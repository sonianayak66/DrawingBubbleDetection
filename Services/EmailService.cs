using MailKit.Net.Imap;
using MailKit;
using MimeKit;
using System.Text.Json;
using Dapper;
using MPCRS.Models;
using System.Data;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace MPCRS.Services
{
    public interface IEmailService
    {
        Task SyncEmailsAsync();
        Task<bool> TestConnectionAsync(string server, int port, bool useSSL, string username, string password);
    }

    public class EmailService : IEmailService
    {
        private readonly MPDapperContext _dapperContext;
        private readonly ILogger<EmailService> _logger;

        public EmailService(MPDapperContext dapperContext, ILogger<EmailService> logger)
        {
            _dapperContext = dapperContext;
            _logger = logger;
        }

        public async Task SyncEmailsAsync()
        {
            try
            {
                // Get active email configurations
                var configs = await GetActiveEmailConfigurationsAsync();

                foreach (var config in configs)
                {
                    await SyncEmailsFromConfigAsync(config);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email sync");
            }
        }

        private async Task<IEnumerable<dynamic>> GetActiveEmailConfigurationsAsync()
        {
            using (var connection = _dapperContext.CreateConnection())
            {
                return await connection.QueryAsync<dynamic>(
                    "sp_TaskManager_EmailConfigurations_GetForSync", // Use the new SP that returns actual password
                    new { IsActive = true },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        private async Task SyncEmailsFromConfigAsync(dynamic config)
        {
            try
            {
                _logger.LogInformation($"Syncing emails from {config.ConfigName}");
                _logger.LogInformation($"Config details - Server: {config.ImapServer}, Port: {config.ImapPort}, Username: {config.Username}");

                using var client = new ImapClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Connect to server
                _logger.LogInformation($"Connecting to {config.ImapServer}:{config.ImapPort}...");
                await client.ConnectAsync(
                    config.ImapServer,
                    (int)config.ImapPort,
                    (bool)config.UseSSL);
                _logger.LogInformation("Connected successfully");

                // Authenticate
                _logger.LogInformation($"Authenticating user: {config.Username}");
                await client.AuthenticateAsync(
                    config.Username,
                    config.Password);
                _logger.LogInformation("Authenticated successfully");

                // Open inbox folder
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);
                _logger.LogInformation($"Opened inbox. Total messages: {inbox.Count}");

                // Calculate date range - Let's try ALL emails for debugging
                _logger.LogInformation("Searching for ALL emails (no date filter for debugging)");
                var uids = await inbox.SearchAsync(MailKit.Search.SearchQuery.All);

                _logger.LogInformation($"Found {uids.Count} total emails in mailbox");

                if (uids.Count == 0)
                {
                    _logger.LogWarning("No emails found in mailbox");
                    await client.DisconnectAsync(true);
                    return;
                }

                // Convert UniqueIdSet to List and process first 5 emails for testing
                var uidList = uids.ToList();
               // var testUids = uidList.Take(5);
                _logger.LogInformation($"Processing first {uidList.Count()} emails for testing");

                foreach (var uid in uidList)
                {
                    try
                    {
                        _logger.LogInformation($"Processing email UID: {uid}");
                        var message = await inbox.GetMessageAsync(uid);
                        _logger.LogInformation($"Email subject: {message.Subject}, From: {message.From}, Date: {message.Date}");

                        await SaveEmailToDatabase(message);
                        _logger.LogInformation($"Email saved successfully: {message.Subject}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing email UID: {uid}");
                    }
                }

                // Update last sync date
                await UpdateLastSyncDateAsync(config.ConfigGUID);
                _logger.LogInformation("Updated LastSyncDate");

                await client.DisconnectAsync(true);

                _logger.LogInformation($"Completed syncing emails from {config.ConfigName}");
                await ProcessRecentEmailsForRelationships();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error syncing emails from {config.ConfigName}: {ex.Message}");
            }
        }
        private async Task ProcessEmailBatchAsync(IMailFolder inbox, IEnumerable<UniqueId> uids, int configId)
        {
            foreach (var uid in uids)
            {
                try
                {
                    var message = await inbox.GetMessageAsync(uid);
                    await SaveEmailToDatabase(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing email UID: {uid}");
                }
            }
        }

        private async Task UpdateOrCreateRelatedEmailNotification(
    string emailGuid,
    object taskGuid,
    string message,
    IDbConnection connection)
        {
            try
            {
                // First, check if a notification already exists for this email
                var existingNotification = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT NotificationId, NotificationType FROM TaskManager_EmailNotifications WHERE EmailGUID = @EmailGUID AND IsDeleted = 0",
                    new { EmailGUID = emailGuid });

                if (existingNotification != null)
                {
                    // Update existing notification to RelatedEmail
                    await connection.ExecuteAsync(@"
                UPDATE TaskManager_EmailNotifications 
                SET NotificationType = 'RelatedEmail',
                    RelatedTaskGUID = @TaskGUID,
                    Message = @Message,
                    IsActionRequired = 1
                WHERE EmailGUID = @EmailGUID AND IsDeleted = 0",
                        new
                        {
                            EmailGUID = emailGuid,
                            TaskGUID = taskGuid?.ToString(),
                            Message = message
                        });

                    _logger.LogInformation($"Updated existing notification to RelatedEmail for email {emailGuid}");
                }
                else
                {
                    // Create new RelatedEmail notification
                    await connection.ExecuteAsync(@"
                INSERT INTO TaskManager_EmailNotifications 
                (NotificationType, EmailGUID, RelatedTaskGUID, Message, IsActionRequired, CreatedDate)
                VALUES ('RelatedEmail', @EmailGUID, @TaskGUID, @Message, 1, GETDATE())",
                        new
                        {
                            EmailGUID = emailGuid,
                            TaskGUID = taskGuid?.ToString(),
                            Message = message
                        });

                    _logger.LogInformation($"Created new RelatedEmail notification for email {emailGuid}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating/creating related email notification");
            }
        }


        public async Task ProcessRecentEmailsForRelationships()
        {
            try
            {
                var settings = await GetApplicationSettingsAsync();
                var lookbackDays = int.Parse(settings["thread_lookback_days"]);
                var lookbackDate = DateTime.Now.AddDays(-lookbackDays);

                using (var connection = _dapperContext.CreateConnection())
                {
                    // **NEW: First, update notifications for emails that already have "Suggested" relationships**
                    _logger.LogInformation("Updating notifications for existing Suggested relationships...");

                    var emailsWithSuggestedRelationships = await connection.QueryAsync<dynamic>(@"
                SELECT e.EmailGUID, e.MessageId, e.Subject, r.TaskGUID, r.ConfidenceScore
                FROM TaskManager_Emails e
                INNER JOIN TaskManager_EmailTaskRelationships r ON e.EmailGUID = r.EmailGUID
                WHERE r.IsDeleted = 0 AND r.RelationshipType = 'Suggested' AND e.IsDeleted = 0");

                    foreach (var emailWithRelationship in emailsWithSuggestedRelationships)
                    {
                        await UpdateOrCreateRelatedEmailNotification(
                            emailWithRelationship.EmailGUID.ToString(),
                            emailWithRelationship.TaskGUID.ToString(),
                            $"Email '{emailWithRelationship.Subject}' is related to existing task (Confidence: {emailWithRelationship.ConfidenceScore:P0})",
                            connection);
                    }

                    _logger.LogInformation($"Updated notifications for {emailsWithSuggestedRelationships.Count()} emails with existing relationships");

                    // **EXISTING: Process emails without relationships**
                    var recentEmailsWithoutRelationships = await connection.QueryAsync<dynamic>(@"
                SELECT e.MessageId, e.InReplyTo, e.[References], e.ThreadId, e.CleanSubject, 
                       e.Subject, e.FromEmail, e.ReceivedDate
                FROM TaskManager_Emails e
                LEFT JOIN TaskManager_EmailTaskRelationships r ON e.EmailGUID = r.EmailGUID AND r.IsDeleted = 0
                WHERE e.IsDeleted = 0 
                  AND e.ReceivedDate >= @LookbackDate
                  AND r.EmailGUID IS NULL
                ORDER BY e.ReceivedDate DESC",
                        new { LookbackDate = lookbackDate });

                    _logger.LogInformation($"Found {recentEmailsWithoutRelationships.Count()} recent emails without relationships");

                    foreach (var email in recentEmailsWithoutRelationships)
                    {
                        await ProcessEmailThreadRelationships(email, settings);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recent emails for relationships");
            }
        }

        private async Task<string> SaveEmailToDatabase(MimeMessage message)
        {
            try
            {
                // Get application settings
                var settings = await GetApplicationSettingsAsync();
                var cleaningStrictness = settings["subject_cleaning_strictness"];

                // Extract threading information
                var messageId = message.MessageId ?? Guid.NewGuid().ToString();
                var inReplyTo = message.InReplyTo;
                var references = message.References != null && message.References.Count > 0
                    ? string.Join(",", message.References)
                    : null;
                var threadId = GenerateThreadId(message);
                var cleanSubject = CleanEmailSubject(message.Subject, cleaningStrictness);

                _logger.LogInformation($"Processing email - MessageId: {messageId}, InReplyTo: {inReplyTo}, ThreadId: {threadId}");

                // Create a more reliable unique identifier for emails
                if (string.IsNullOrEmpty(messageId))
                {
                    var fromEmail = message.From.Mailboxes.FirstOrDefault()?.Address ?? "";
                    var subject = message.Subject ?? "";
                    var dateString = message.Date.ToString("yyyy-MM-dd HH:mm:ss");

                    messageId = $"{fromEmail}|{subject}|{dateString}".GetHashCode().ToString();
                    _logger.LogWarning($"Generated MessageId for email: {subject} -> {messageId}");
                }

                var emailData = new
                {
                    MessageId = messageId,
                    InReplyTo = inReplyTo,
                    References = references,
                    ThreadId = threadId,
                    CleanSubject = cleanSubject,
                    Subject = message.Subject ?? "",
                    FromEmail = message.From.Mailboxes.FirstOrDefault()?.Address ?? "",
                    FromName = message.From.Mailboxes.FirstOrDefault()?.Name ?? "",
                    ToEmails = JsonSerializer.Serialize(message.To.Mailboxes.Select(m => new { Name = m.Name, Address = m.Address })),
                    CcEmails = JsonSerializer.Serialize(message.Cc.Mailboxes.Select(m => new { Name = m.Name, Address = m.Address })),
                    EmailBodyText = message.TextBody ?? "",
                    EmailBodyHtml = message.HtmlBody ?? "",
                    ReceivedDate = message.Date.DateTime,
                    HasAttachments = message.Attachments.Any()
                };

                using (var connection = _dapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_TaskManager_Emails_Save",
                        new { JsonData = JsonSerializer.Serialize(emailData) },
                        commandType: System.Data.CommandType.StoredProcedure);

                    var resultStatus = result?.Result?.ToString() ?? "ERROR";

                    _logger.LogInformation($"Email save result: {resultStatus} for subject: {message.Subject}");

                    if (resultStatus == "SUCCESS")
                    {
                        _logger.LogInformation($"New email saved: {message.Subject} (Thread: {threadId})");

                        // After saving email, check for thread relationships and create notifications
                        await ProcessEmailThreadRelationships(emailData, settings);
                        await EnsureEmailNotificationExists(emailData.MessageId, connection, settings);
                        return "SUCCESS";
                    }
                    else if (resultStatus == "DUPLICATE")
                    {
                        _logger.LogInformation($"Duplicate email skipped: {message.Subject} (MessageId: {messageId})");
                        return "DUPLICATE";
                    }
                    else
                    {
                        _logger.LogError($"Error saving email: {message.Subject} - Result: {resultStatus}");
                        return "ERROR";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving email to database: {message.Subject}");
                return "ERROR";
            }
        }


        private async Task EnsureEmailNotificationExists(string messageId, IDbConnection connection, Dictionary<string, string> settings)
        {
            try
            {
                // Check if email already has a notification
                var existingNotification = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT n.NotificationId 
            FROM TaskManager_EmailNotifications n
            INNER JOIN TaskManager_Emails e ON n.EmailGUID = e.EmailGUID
            WHERE e.MessageId = @MessageId AND e.IsDeleted = 0 AND n.IsDeleted = 0",
                    new { MessageId = messageId });

                if (existingNotification == null)
                {
                    // No notification exists, create a NewEmail notification
                    await CreateEmailNotification(
                        "NewEmail",
                        messageId,
                        null,
                        "New email requires review",
                        connection);

                    _logger.LogInformation($"Created fallback NewEmail notification for {messageId}");
                }
                else
                {
                    _logger.LogInformation($"Notification already exists for {messageId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring email notification exists");
            }
        }


        private async Task ProcessEmailThreadRelationships(dynamic emailData, Dictionary<string, string> settings)
        {
            try
            {
                _logger.LogInformation($"Processing thread relationships for email: {emailData.Subject}");
                _logger.LogInformation($"Email ThreadId: {emailData.ThreadId}, InReplyTo: {emailData.InReplyTo}, MessageId: {emailData.MessageId}");

                var confidenceThreshold = decimal.Parse(settings["thread_confidence_threshold"]);
                var lookbackDays = int.Parse(settings["thread_lookback_days"]);
                var lookbackDate = DateTime.Now.AddDays(-lookbackDays);

                _logger.LogInformation($"Using confidence threshold: {confidenceThreshold}, lookback days: {lookbackDays}");

                using (var connection = _dapperContext.CreateConnection())
                {
                    // Get EmailGUID for this email
                    var currentEmailGuid = await connection.QueryFirstOrDefaultAsync<string>(
                        "SELECT CAST(EmailGUID AS NVARCHAR(50)) FROM TaskManager_Emails WHERE MessageId = @MessageId AND IsDeleted = 0",
                        new { MessageId = emailData.MessageId });

                    if (string.IsNullOrEmpty(currentEmailGuid))
                    {
                        _logger.LogWarning($"Could not find EmailGUID for MessageId: {emailData.MessageId}");
                        return;
                    }

                    // Check if this email already has relationships
                    var existingRelationship = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT RelationshipType FROM TaskManager_EmailTaskRelationships WHERE EmailGUID = @EmailGUID AND IsDeleted = 0",
                        new { EmailGUID = currentEmailGuid });

                    if (existingRelationship != null)
                    {
                        _logger.LogInformation($"Email {emailData.Subject} already has relationship: {existingRelationship.RelationshipType}");
                        return; // Skip if already processed
                    }

                    // Find converted emails in the same thread (same as before)
                    var relatedEmails = await connection.QueryAsync<dynamic>(@"
                SELECT e.EmailGUID, e.MessageId, e.ThreadId, e.CleanSubject, e.Subject, e.FromEmail,
                       r.TaskGUID, r.RelationshipType
                FROM TaskManager_Emails e
                LEFT JOIN TaskManager_EmailTaskRelationships r ON e.EmailGUID = r.EmailGUID 
                    AND r.IsDeleted = 0 AND r.RelationshipType = 'Converted'
                WHERE e.IsDeleted = 0 
                  AND e.ReceivedDate >= @LookbackDate
                  AND e.MessageId != @CurrentMessageId
                  AND (
                    e.ThreadId = @ThreadId 
                    OR e.InReplyTo = @MessageId
                    OR e.MessageId = @InReplyTo
                  )",
                        new
                        {
                            LookbackDate = lookbackDate,
                            CurrentMessageId = emailData.MessageId,
                            ThreadId = emailData.ThreadId,
                            InReplyTo = emailData.InReplyTo ?? "",
                            MessageId = emailData.MessageId
                        });

                    var convertedEmailsInThread = relatedEmails.Where(e => e.TaskGUID != null).ToList();

                    _logger.LogInformation($"Found {convertedEmailsInThread.Count} converted emails in this thread");

                    if (convertedEmailsInThread.Any())
                    {
                        // Found emails in this thread that are already converted to tasks
                        foreach (var convertedEmail in convertedEmailsInThread)
                        {
                            var confidence = CalculateConfidenceScore(emailData, convertedEmail);

                            _logger.LogInformation($"Confidence score for relationship: {confidence} (threshold: {confidenceThreshold})");

                            if (confidence >= confidenceThreshold)
                            {
                                // Create suggested relationship
                                await CreateEmailTaskRelationship(
                                    emailData.MessageId,
                                    convertedEmail.TaskGUID,
                                    "Suggested",
                                    confidence,
                                    connection);

                                // **NEW: Update existing notification or create RelatedEmail notification**
                                await UpdateOrCreateRelatedEmailNotification(
                                    currentEmailGuid,
                                    convertedEmail.TaskGUID,
                                    $"Email '{emailData.Subject}' is related to existing task (Confidence: {confidence:P0})",
                                    connection);

                                _logger.LogInformation($"Created thread relationship and updated notification: Email {emailData.Subject} -> Task {convertedEmail.TaskGUID}");
                                return; // Exit after creating first relationship
                            }
                        }
                    }
                    else
                    {
                        // No related converted emails found - keep as NewEmail notification
                        _logger.LogInformation($"No related converted emails found for: {emailData.Subject}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email thread relationships");
            }
        }




        private decimal CalculateConfidenceScore(dynamic newEmail, dynamic existingEmail)
        {
            decimal score = 0m;

            var newThreadId = newEmail.ThreadId?.ToString() ?? "";
            var existingThreadId = existingEmail.ThreadId?.ToString() ?? "";
            var newMessageId = newEmail.MessageId?.ToString() ?? "";
            var existingMessageId = existingEmail.MessageId?.ToString() ?? "";
            var newInReplyTo = newEmail.InReplyTo?.ToString() ?? "";

            _logger.LogInformation($"Calculating confidence: NewThreadId={newThreadId}, ExistingThreadId={existingThreadId}");
            _logger.LogInformation($"NewMessageId={newMessageId}, ExistingMessageId={existingMessageId}, NewInReplyTo={newInReplyTo}");

            // 1. Direct reply relationship (highest confidence)
            if (!string.IsNullOrEmpty(newInReplyTo) && newInReplyTo == existingMessageId)
            {
                score += 0.8m; // 80% for direct reply
                _logger.LogInformation($"Direct reply match: +0.8, total={score}");
            }
            // 2. Reverse - existing email replies to new email
            else if (!string.IsNullOrEmpty(existingEmail.InReplyTo?.ToString()) &&
                     existingEmail.InReplyTo.ToString() == newMessageId)
            {
                score += 0.8m; // 80% for reverse reply
                _logger.LogInformation($"Reverse reply match: +0.8, total={score}");
            }
            // 3. Same ThreadId (high confidence)
            else if (!string.IsNullOrEmpty(newThreadId) && !string.IsNullOrEmpty(existingThreadId) &&
                     newThreadId == existingThreadId)
            {
                score += 0.7m; // 70% for exact thread match
                _logger.LogInformation($"Exact ThreadId match: +0.7, total={score}");
            }

            // 4. Clean subject match (medium confidence)
            if (!string.IsNullOrEmpty(newEmail.CleanSubject?.ToString()) &&
                !string.IsNullOrEmpty(existingEmail.CleanSubject?.ToString()) &&
                newEmail.CleanSubject.ToString().Equals(existingEmail.CleanSubject?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                score += 0.2m; // 20% for subject match
                _logger.LogInformation($"Subject match: +0.2, total={score}");
            }

            // 5. Same sender (lower confidence)
            if (!string.IsNullOrEmpty(newEmail.FromEmail?.ToString()) &&
                !string.IsNullOrEmpty(existingEmail.FromEmail?.ToString()) &&
                newEmail.FromEmail.ToString().Equals(existingEmail.FromEmail?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                score += 0.1m; // 10% for same sender
                _logger.LogInformation($"Same sender: +0.1, total={score}");
            }

            // Cap at 100%
            var finalScore = Math.Min(score, 1.0m);
            _logger.LogInformation($"Final confidence score: {finalScore}");
            return finalScore;
        }

        private async Task CreateEmailTaskRelationship(
    string emailMessageId,
    object taskGuidObj, // Change from string to object
    string relationshipType,
    decimal confidenceScore,
    IDbConnection connection)
        {
            try
            {
                // Convert the TaskGUID properly
                string taskGuid = taskGuidObj?.ToString();

                if (string.IsNullOrEmpty(taskGuid))
                {
                    _logger.LogWarning($"TaskGUID is null or empty for email {emailMessageId}");
                    return;
                }

                // First get the EmailGUID from MessageId
                var emailGuid = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT CAST(EmailGUID AS NVARCHAR(50)) FROM TaskManager_Emails WHERE MessageId = @MessageId AND IsDeleted = 0",
                    new { MessageId = emailMessageId });

                if (emailGuid != null)
                {
                    await connection.ExecuteAsync(@"
                INSERT INTO TaskManager_EmailTaskRelationships 
                (EmailGUID, TaskGUID, RelationshipType, ConfidenceScore, CreatedDate)
                VALUES (@EmailGUID, @TaskGUID, @RelationshipType, @ConfidenceScore, GETDATE())",
                        new
                        {
                            EmailGUID = emailGuid,
                            TaskGUID = taskGuid,
                            RelationshipType = relationshipType,
                            ConfidenceScore = confidenceScore
                        });

                    _logger.LogInformation($"Created relationship: {relationshipType} between email {emailMessageId} and task {taskGuid}");
                }
                else
                {
                    _logger.LogWarning($"Could not find email with MessageId: {emailMessageId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email-task relationship");
            }
        }




        private async Task CreateEmailNotification(
      string notificationType,
      string emailMessageId,
      string relatedTaskGuid,
      string message,
      IDbConnection connection)
        {
            try
            {
                // First get the EmailGUID from MessageId
                var emailGuid = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT CAST(EmailGUID AS NVARCHAR(50)) FROM TaskManager_Emails WHERE MessageId = @MessageId AND IsDeleted = 0",
                    new { MessageId = emailMessageId });

                if (emailGuid != null)
                {
                    // Just execute the insert without trying to return the EmailGUID
                    await connection.ExecuteAsync(@"
                INSERT INTO TaskManager_EmailNotifications 
                (NotificationType, EmailGUID, RelatedTaskGUID, Message, IsActionRequired, CreatedDate)
                VALUES (@NotificationType, @EmailGUID, @RelatedTaskGUID, @Message, @IsActionRequired, GETDATE())",
                        new
                        {
                            NotificationType = notificationType,
                            EmailGUID = emailGuid,
                            RelatedTaskGUID = relatedTaskGuid,
                            Message = message,
                            IsActionRequired = notificationType == "RelatedEmail" ? 1 : 0
                        });

                    _logger.LogInformation($"Created notification: {notificationType} for email {emailMessageId}");
                }
                else
                {
                    _logger.LogWarning($"Could not find email with MessageId: {emailMessageId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email notification");
            }
        }
        private async Task UpdateLastSyncDateAsync(Guid configGuid)
        {
            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    await connection.ExecuteAsync(
                        "UPDATE TaskManager_EmailConfigurations SET LastSyncDate = @SyncDate WHERE ConfigGUID = @ConfigGUID",
                        new { SyncDate = DateTime.UtcNow, ConfigGUID = configGuid });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last sync date");
            }
        }

        public async Task<bool> TestConnectionAsync(string server, int port, bool useSSL, string username, string password)
        {
            try
            {
                using var client = new ImapClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(server, port, useSSL);
                await client.AuthenticateAsync(username, password);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email connection test failed");
                return false;
            }
        }


        // Add these helper methods to the EmailService class

        private string CleanEmailSubject(string subject, string strictness = "lenient")
        {
            if (string.IsNullOrEmpty(subject)) return "";

            var cleaned = subject.Trim();

            // Remove common prefixes based on strictness level
            var prefixes = strictness switch
            {
                "strict" => new[] { "RE:", "FW:", "FWD:" },
                "lenient" => new[] { "RE:", "Re:", "FW:", "Fw:", "FWD:", "Fwd:", "REPLY:", "Reply:" },
                "loose" => new[] { "RE:", "Re:", "re:", "FW:", "Fw:", "fw:", "FWD:", "Fwd:", "fwd:",
                          "REPLY:", "Reply:", "reply:", "FORWARD:", "Forward:", "forward:" },
                _ => new[] { "RE:", "Re:", "FW:", "Fw:", "FWD:", "Fwd:" }
            };

            foreach (var prefix in prefixes)
            {
                while (cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    cleaned = cleaned.Substring(prefix.Length).Trim();
                }
            }

            // Remove multiple spaces and normalize
            return System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim();
        }

        private string GenerateThreadId(MimeMessage message)
        {
            // Use the original Message-ID if it's the first email in thread
            if (string.IsNullOrEmpty(message.InReplyTo))
            {
                return message.MessageId ?? Guid.NewGuid().ToString();
            }

            // For replies, use the original Message-ID from References or InReplyTo
            if (!string.IsNullOrEmpty(message.InReplyTo))
            {
                return message.InReplyTo;
            }

            // Fallback to first reference if available
            if (message.References?.Count > 0)
            {
                return message.References[0];
            }

            return message.MessageId ?? Guid.NewGuid().ToString();
        }

        private async Task<Dictionary<string, string>> GetApplicationSettingsAsync()
        {
            var settings = new Dictionary<string, string>();

            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    var dbSettings = await connection.QueryAsync<dynamic>(
                        "SELECT SettingKey, SettingValue FROM TaskManager_ApplicationSettings WHERE IsActive = 1");

                    foreach (var setting in dbSettings)
                    {
                        settings[setting.SettingKey] = setting.SettingValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading application settings, using defaults");
            }

            // Set defaults if not found
            if (!settings.ContainsKey("thread_confidence_threshold")) settings["thread_confidence_threshold"] = "0.70";
            if (!settings.ContainsKey("thread_lookback_days")) settings["thread_lookback_days"] = "30";
            if (!settings.ContainsKey("subject_cleaning_strictness")) settings["subject_cleaning_strictness"] = "lenient";

            return settings;
        }



    }
}