import * as yup from "yup";

export const taskValidationSchema = yup.object({
  TaskTitle: yup
    .string()
    .required("Task title is required")
    .min(3, "Task title must be at least 3 characters")
    .max(200, "Task title cannot exceed 200 characters"),

  TaskDescription: yup
    .string()
    .max(2000, "Description cannot exceed 2000 characters"),

  ProjectGUID: yup.string().nullable().required("Please select a project"),

  BucketGUID: yup.string().nullable().required("Please select a status/bucket"),

  Priority: yup
    .string()
    .oneOf(
      ["Critical", "High", "Medium", "Low"],
      "Please select a valid priority"
    ),

  ProgressPercentage: yup
    .number()
    .min(0, "Progress cannot be negative")
    .max(100, "Progress cannot exceed 100%")
    .integer("Progress must be a whole number"),

  EstimatedHours: yup
    .number()
    .nullable()
    .min(0, "Estimated hours cannot be negative")
    .max(1000, "Estimated hours seems too high"),

  // In taskValidationSchema.js, update the date validations:

  StartDate: yup
    .date()
    .nullable()
    .transform((value, originalValue) => {
      // Handle null, undefined, empty string
      if (!originalValue || originalValue === "") return null;
      // Handle Date objects
      if (originalValue instanceof Date) return originalValue;
      // Handle string dates
      if (typeof originalValue === "string") {
        const parsed = new Date(originalValue);
        return isNaN(parsed.getTime()) ? null : parsed;
      }
      return null;
    }),

  DueDate: yup
    .date()
    .nullable()
    .transform((value, originalValue) => {
      // Handle null, undefined, empty string
      if (!originalValue || originalValue === "") return null;
      // Handle Date objects
      if (originalValue instanceof Date) return originalValue;
      // Handle string dates
      if (typeof originalValue === "string") {
        const parsed = new Date(originalValue);
        return isNaN(parsed.getTime()) ? null : parsed;
      }
      return null;
    })
    .when("StartDate", (startDate, schema) => {
      return startDate && startDate[0]
        ? schema.min(startDate[0], "Due date must be after start date")
        : schema;
    }),

  Tags: yup.string().max(500, "Tags cannot exceed 500 characters"),
});
