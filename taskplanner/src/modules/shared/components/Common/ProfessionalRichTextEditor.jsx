import React, { useRef, useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Button,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Divider,
  ButtonGroup,
} from '@mui/material';
import {
  FormatBold,
  FormatItalic,
  FormatUnderlined,
  FormatStrikethrough,
  FormatListBulleted,
  FormatListNumbered,
  FormatAlignLeft,
  FormatAlignCenter,
  FormatAlignRight,
  FormatAlignJustify,
  FormatIndentDecrease,
  FormatIndentIncrease,
  FormatColorText,
  FormatColorFill,
  Link,
  Image,
  Table,
  Undo,
  Redo,
  FormatSize,
  Code,
  FormatQuote,
  Subscript,
  Superscript,
  InsertDriveFile,
  ContentPaste,
  ContentCopy,
  ContentCut,
  Print,
  Save,
} from '@mui/icons-material';

const ProfessionalRichTextEditor = ({
  value = '',
  onChange,
  placeholder = "Start typing or paste content from MS Word...",
  disabled = false,
  label = "Content",
  height = 500
}) => {
  const editorRef = useRef(null);
  const fileInputRef = useRef(null);
  const [linkDialogOpen, setLinkDialogOpen] = useState(false);
  const [linkUrl, setLinkUrl] = useState('');
  const [linkText, setLinkText] = useState('');
  const [tableDialogOpen, setTableDialogOpen] = useState(false);
  const [tableRows, setTableRows] = useState(3);
  const [tableCols, setTableCols] = useState(3);

  useEffect(() => {
    if (editorRef.current && value !== editorRef.current.innerHTML) {
      editorRef.current.innerHTML = value || '';
    }
  }, [value]);

  const executeCommand = (command, value = null) => {
    try {
      editorRef.current.focus();
      document.execCommand(command, false, value);
      handleContentChange();
    } catch (error) {
      console.warn('Command not supported:', command);
    }
  };

  const handleContentChange = () => {
    if (editorRef.current && onChange) {
      onChange(editorRef.current.innerHTML);
    }
  };

  // Handle paste events - especially from MS Word
  const handlePaste = (e) => {
    e.preventDefault();
    
    const clipboardData = e.clipboardData || window.clipboardData;
    
    // Handle different types of pasted content
    if (clipboardData.types.includes('text/html')) {
      // Rich text from Word or other applications
      let html = clipboardData.getData('text/html');
      
      // Clean up MS Word specific tags and styling
      html = cleanWordHTML(html);
      
      // Insert cleaned HTML
      executeCommand('insertHTML', html);
    } else if (clipboardData.types.includes('text/plain')) {
      // Plain text
      const text = clipboardData.getData('text/plain');
      executeCommand('insertText', text);
    }

    // Handle pasted images
    const items = Array.from(clipboardData.items);
    const imageItems = items.filter(item => item.type.startsWith('image/'));
    
    if (imageItems.length > 0) {
      imageItems.forEach(item => {
        const file = item.getAsFile();
        handleImageFile(file);
      });
    }
  };

  // Clean MS Word HTML
  const cleanWordHTML = (html) => {
    // Remove Word-specific tags and attributes
    html = html.replace(/<o:p\s*\/?>|<\/o:p>/gi, '');
    html = html.replace(/<w:[^>]*>|<\/w:[^>]*>/gi, '');
    html = html.replace(/class=["'][^"']*["']/gi, '');
    html = html.replace(/style=["'][^"']*["']/gi, '');
    html = html.replace(/lang=["'][^"']*["']/gi, '');
    html = html.replace(/xml:[^=]*=["'][^"']*["']/gi, '');
    html = html.replace(/<span[^>]*>\s*<\/span>/gi, '');
    html = html.replace(/<p[^>]*>\s*<\/p>/gi, '');
    
    // Clean up excessive whitespace and line breaks
    html = html.replace(/\r\n|\r|\n/g, ' ');
    html = html.replace(/\s+/g, ' ');
    html = html.trim();
    
    return html;
  };

  // Handle image files (from paste or file upload)
  const handleImageFile = (file) => {
    if (!file || !file.type.startsWith('image/')) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      const img = `<img src="${e.target.result}" style="max-width: 100%; height: auto; margin: 10px 0;" alt="Pasted image" />`;
      executeCommand('insertHTML', img);
    };
    reader.readAsDataURL(file);
  };

  // Handle drag and drop for images
  const handleDrop = (e) => {
    e.preventDefault();
    const files = Array.from(e.dataTransfer.files);
    const imageFiles = files.filter(file => file.type.startsWith('image/'));
    
    imageFiles.forEach(handleImageFile);
  };

  const handleDragOver = (e) => {
    e.preventDefault();
  };

  // Insert link
  const insertLink = () => {
    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
      const selectedText = selection.toString();
      setLinkText(selectedText || 'Link text');
      setLinkUrl('https://');
      setLinkDialogOpen(true);
    }
  };

  const confirmLink = () => {
    if (linkUrl && linkText) {
      const link = `<a href="${linkUrl}" target="_blank">${linkText}</a>`;
      executeCommand('insertHTML', link);
    }
    setLinkDialogOpen(false);
    setLinkUrl('');
    setLinkText('');
  };

  // Insert table
  const insertTable = () => {
    setTableDialogOpen(true);
  };

  const confirmTable = () => {
    let tableHTML = '<table border="1" style="border-collapse: collapse; width: 100%; margin: 10px 0;">';
    
    // Create header row
    tableHTML += '<tr>';
    for (let j = 0; j < tableCols; j++) {
      tableHTML += `<th style="padding: 8px; background-color: #f5f5f5; border: 1px solid #ddd;">Header ${j + 1}</th>`;
    }
    tableHTML += '</tr>';
    
    // Create data rows
    for (let i = 1; i < tableRows; i++) {
      tableHTML += '<tr>';
      for (let j = 0; j < tableCols; j++) {
        tableHTML += `<td style="padding: 8px; border: 1px solid #ddd;">Cell ${i},${j + 1}</td>`;
      }
      tableHTML += '</tr>';
    }
    
    tableHTML += '</table>';
    executeCommand('insertHTML', tableHTML);
    setTableDialogOpen(false);
  };

  // Handle keyboard shortcuts
  const handleKeyDown = (e) => {
    if (e.ctrlKey || e.metaKey) {
      switch (e.key.toLowerCase()) {
        case 'b':
          e.preventDefault();
          executeCommand('bold');
          break;
        case 'i':
          e.preventDefault();
          executeCommand('italic');
          break;
        case 'u':
          e.preventDefault();
          executeCommand('underline');
          break;
        case 'z':
          e.preventDefault();
          executeCommand('undo');
          break;
        case 'y':
          e.preventDefault();
          executeCommand('redo');
          break;
        case 'k':
          e.preventDefault();
          insertLink();
          break;
        case 's':
          e.preventDefault();
          handleContentChange(); // Trigger save
          break;
      }
    }
    
    // Handle Enter key for better paragraph formatting
    if (e.key === 'Enter' && !e.shiftKey) {
      // Let default behavior handle paragraph creation
    }
  };

  const fontFamilies = [
    'Arial', 'Helvetica', 'Times New Roman', 'Courier New', 'Verdana', 
    'Georgia', 'Palatino', 'Garamond', 'Bookman', 'Comic Sans MS',
    'Trebuchet MS', 'Arial Black', 'Impact', 'Tahoma', 'Calibri'
  ];

  const fontSizes = ['8px', '9px', '10px', '11px', '12px', '14px', '16px', '18px', '20px', '22px', '24px', '26px', '28px', '36px', '48px', '72px'];

  const colors = [
    '#000000', '#FFFFFF', '#FF0000', '#00FF00', '#0000FF', '#FFFF00',
    '#FF00FF', '#00FFFF', '#800000', '#008000', '#000080', '#808000',
    '#800080', '#008080', '#C0C0C0', '#808080', '#FFA500', '#A52A2A',
    '#DDA0DD', '#98FB98', '#87CEEB', '#F0E68C', '#FFB6C1', '#20B2AA'
  ];

  if (disabled) {
    return (
      <Box>
        <Typography variant="subtitle2" gutterBottom>
          {label}
        </Typography>
        <Box
          sx={{
            border: '1px solid',
            borderColor: 'action.disabled',
            borderRadius: 1,
            p: 2,
            minHeight: height,
            backgroundColor: 'action.disabledBackground',
            color: 'text.disabled',
          }}
          dangerouslySetInnerHTML={{ __html: value || '' }}
        />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="subtitle2" gutterBottom>
        {label} *
      </Typography>

      {/* Professional Toolbar */}
      <Paper sx={{ p: 1, mb: 1, border: '1px solid', borderColor: 'divider' }}>
        {/* Row 1: File Operations */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1, flexWrap: 'wrap' }}>
          <ButtonGroup size="small">
            <Tooltip title="Undo (Ctrl+Z)">
              <IconButton onClick={() => executeCommand('undo')} size="small">
                <Undo />
              </IconButton>
            </Tooltip>
            <Tooltip title="Redo (Ctrl+Y)">
              <IconButton onClick={() => executeCommand('redo')} size="small">
                <Redo />
              </IconButton>
            </Tooltip>
          </ButtonGroup>

          <Divider orientation="vertical" flexItem />

          <ButtonGroup size="small">
            <Tooltip title="Cut (Ctrl+X)">
              <IconButton onClick={() => executeCommand('cut')} size="small">
                <ContentCut />
              </IconButton>
            </Tooltip>
            <Tooltip title="Copy (Ctrl+C)">
              <IconButton onClick={() => executeCommand('copy')} size="small">
                <ContentCopy />
              </IconButton>
            </Tooltip>
            <Tooltip title="Paste (Ctrl+V)">
              <IconButton onClick={() => executeCommand('paste')} size="small">
                <ContentPaste />
              </IconButton>
            </Tooltip>
          </ButtonGroup>

          <Divider orientation="vertical" flexItem />

          <Button
            size="small"
            startIcon={<Image />}
            onClick={() => fileInputRef.current?.click()}
          >
            Insert Image
          </Button>
          <input
            type="file"
            ref={fileInputRef}
            accept="image/*"
            style={{ display: 'none' }}
            onChange={(e) => handleImageFile(e.target.files[0])}
          />
        </Box>

        {/* Row 2: Font Formatting */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1, flexWrap: 'wrap' }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <Select
              defaultValue="Arial"
              onChange={(e) => executeCommand('fontName', e.target.value)}
              displayEmpty
            >
              {fontFamilies.map((font) => (
                <MenuItem key={font} value={font} style={{ fontFamily: font }}>
                  {font}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 60 }}>
            <Select
              defaultValue="14px"
              onChange={(e) => executeCommand('fontSize', e.target.value)}
              displayEmpty
            >
              {fontSizes.map((size) => (
                <MenuItem key={size} value={size.replace('px', '')}>
                  {size}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <Divider orientation="vertical" flexItem />

          <ButtonGroup size="small">
            <Tooltip title="Bold (Ctrl+B)">
              <IconButton onClick={() => executeCommand('bold')} size="small">
                <FormatBold />
              </IconButton>
            </Tooltip>
            <Tooltip title="Italic (Ctrl+I)">
              <IconButton onClick={() => executeCommand('italic')} size="small">
                <FormatItalic />
              </IconButton>
            </Tooltip>
            <Tooltip title="Underline (Ctrl+U)">
              <IconButton onClick={() => executeCommand('underline')} size="small">
                <FormatUnderlined />
              </IconButton>
            </Tooltip>
            <Tooltip title="Strikethrough">
              <IconButton onClick={() => executeCommand('strikeThrough')} size="small">
                <FormatStrikethrough />
              </IconButton>
            </Tooltip>
            <Tooltip title="Subscript">
              <IconButton onClick={() => executeCommand('subscript')} size="small">
                <Subscript />
              </IconButton>
            </Tooltip>
            <Tooltip title="Superscript">
              <IconButton onClick={() => executeCommand('superscript')} size="small">
                <Superscript />
              </IconButton>
            </Tooltip>
          </ButtonGroup>

          <Divider orientation="vertical" flexItem />

          {/* Color Palette */}
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', maxWidth: 200, gap: 1 }}>
              <Tooltip title="Text Color">
                <FormatColorText fontSize="small" />
              </Tooltip>
              {colors.slice(0, 12).map((color) => (
                <Box
                  key={color}
                  onClick={() => executeCommand('foreColor', color)}
                  sx={{
                    width: 16,
                    height: 16,
                    backgroundColor: color,
                    cursor: 'pointer',
                    border: '1px solid #ccc',
                    borderRadius: 1,
                    '&:hover': { transform: 'scale(1.1)' }
                  }}
                />
              ))}
            </Box>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', maxWidth: 200, gap: 1 }}>
              <Tooltip title="Highlight Color">
                <FormatColorFill fontSize="small" />
              </Tooltip>
              {colors.slice(12).map((color) => (
                <Box
                  key={color}
                  onClick={() => executeCommand('hiliteColor', color)}
                  sx={{
                    width: 16,
                    height: 16,
                    backgroundColor: color,
                    cursor: 'pointer',
                    border: '1px solid #ccc',
                    borderRadius: 1,
                    '&:hover': { transform: 'scale(1.1)' }
                  }}
                />
              ))}
            </Box>
          </Box>
        </Box>

        {/* Row 3: Alignment and Lists */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1, flexWrap: 'wrap' }}>
          <ButtonGroup size="small">
            <Tooltip title="Align Left">
              <IconButton onClick={() => executeCommand('justifyLeft')} size="small">
                <FormatAlignLeft />
              </IconButton>
            </Tooltip>
            <Tooltip title="Center">
              <IconButton onClick={() => executeCommand('justifyCenter')} size="small">
                <FormatAlignCenter />
              </IconButton>
            </Tooltip>
            <Tooltip title="Align Right">
              <IconButton onClick={() => executeCommand('justifyRight')} size="small">
                <FormatAlignRight />
              </IconButton>
            </Tooltip>
            <Tooltip title="Justify">
              <IconButton onClick={() => executeCommand('justifyFull')} size="small">
                <FormatAlignJustify />
              </IconButton>
            </Tooltip>
          </ButtonGroup>

          <Divider orientation="vertical" flexItem />

          <ButtonGroup size="small">
            <Tooltip title="Bullet List">
              <IconButton onClick={() => executeCommand('insertUnorderedList')} size="small">
                <FormatListBulleted />
              </IconButton>
            </Tooltip>
            <Tooltip title="Numbered List">
              <IconButton onClick={() => executeCommand('insertOrderedList')} size="small">
                <FormatListNumbered />
              </IconButton>
            </Tooltip>
          </ButtonGroup>

          <Divider orientation="vertical" flexItem />

          <ButtonGroup size="small">
            <Tooltip title="Decrease Indent">
              <IconButton onClick={() => executeCommand('outdent')} size="small">
                <FormatIndentDecrease />
              </IconButton>
            </Tooltip>
            <Tooltip title="Increase Indent">
              <IconButton onClick={() => executeCommand('indent')} size="small">
                <FormatIndentIncrease />
              </IconButton>
            </Tooltip>
          </ButtonGroup>

          <Divider orientation="vertical" flexItem />

          <ButtonGroup size="small">
            <Tooltip title="Insert Link (Ctrl+K)">
              <IconButton onClick={insertLink} size="small">
                <Link />
              </IconButton>
            </Tooltip>
            <Tooltip title="Insert Table">
              <IconButton onClick={insertTable} size="small">
                <Table />
              </IconButton>
            </Tooltip>
            <Tooltip title="Blockquote">
              <IconButton onClick={() => executeCommand('formatBlock', 'blockquote')} size="small">
                <FormatQuote />
              </IconButton>
            </Tooltip>
            <Tooltip title="Code Block">
              <IconButton onClick={() => executeCommand('formatBlock', 'pre')} size="small">
                <Code />
              </IconButton>
            </Tooltip>
          </ButtonGroup>
        </Box>
      </Paper>

      {/* Editor Content Area */}
      <Box
        ref={editorRef}
        contentEditable={!disabled}
        onInput={handleContentChange}
        onBlur={handleContentChange}
        onPaste={handlePaste}
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onKeyDown={handleKeyDown}
        sx={{
          border: '2px solid',
          borderColor: 'divider',
          borderRadius: 1,
          p: 3,
          minHeight: height,
          maxHeight: height * 1.5,
          overflowY: 'auto',
          outline: 'none',
          backgroundColor: 'background.paper',
          lineHeight: 1.6,
          fontSize: '14px',
          fontFamily: 'Arial, sans-serif',
          '&:focus': {
            borderColor: 'primary.main',
          },
          '&:empty:before': {
            content: `"${placeholder}"`,
            color: 'text.disabled',
            fontStyle: 'italic',
            pointerEvents: 'none',
          },
          // Enhanced styles for better Word compatibility
          '& p': {
            margin: '0 0 10px 0',
            lineHeight: 1.6,
          },
          '& h1, & h2, & h3, & h4, & h5, & h6': {
            margin: '15px 0 10px 0',
            fontWeight: 'bold',
          },
          '& table': {
            borderCollapse: 'collapse',
            width: '100%',
            margin: '15px 0',
            '& th, & td': {
              border: '1px solid #ddd',
              padding: '10px',
              textAlign: 'left',
            },
            '& th': {
              backgroundColor: '#f5f5f5',
              fontWeight: 'bold',
            }
          },
          '& img': {
            maxWidth: '100%',
            height: 'auto',
            margin: '10px 0',
            borderRadius: '4px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
          },
          '& a': {
            color: 'primary.main',
            textDecoration: 'underline',
            '&:hover': {
              textDecoration: 'none',
            }
          },
          '& ul, & ol': {
            paddingLeft: '25px',
            margin: '10px 0',
          },
          '& li': {
            marginBottom: '5px',
            lineHeight: 1.5,
          },
          '& blockquote': {
            borderLeft: '4px solid #ddd',
            paddingLeft: '15px',
            margin: '15px 0',
            fontStyle: 'italic',
            color: 'text.secondary',
          },
          '& pre': {
            backgroundColor: '#f5f5f5',
            padding: '15px',
            borderRadius: '4px',
            overflow: 'auto',
            fontFamily: 'Monaco, Consolas, monospace',
            fontSize: '13px',
          }
        }}
        suppressContentEditableWarning={true}
      />

      {/* Status Bar */}
      <Box sx={{ 
        mt: 1, 
        p: 1, 
        backgroundColor: 'action.hover', 
        borderRadius: 1,
        display: 'flex',
        justifyContent: 'space-between',
        fontSize: '12px',
        color: 'text.secondary'
      }}>
        <span>Paste from MS Word • Drag & drop images • Ctrl+K for links</span>
        <span>Word count: {editorRef.current ? editorRef.current.textContent.length : 0} characters</span>
      </Box>

      {/* Link Dialog */}
      <Dialog open={linkDialogOpen} onClose={() => setLinkDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Insert Link</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <TextField
            label="Link Text"
            value={linkText}
            onChange={(e) => setLinkText(e.target.value)}
            fullWidth
            sx={{ mb: 2 }}
          />
          <TextField
            label="URL"
            value={linkUrl}
            onChange={(e) => setLinkUrl(e.target.value)}
            fullWidth
            placeholder="https://example.com"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setLinkDialogOpen(false)}>Cancel</Button>
          <Button onClick={confirmLink} variant="contained">Insert Link</Button>
        </DialogActions>
      </Dialog>

      {/* Table Dialog */}
      <Dialog open={tableDialogOpen} onClose={() => setTableDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Insert Table</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Rows"
              type="number"
              value={tableRows}
              onChange={(e) => setTableRows(Math.max(1, parseInt(e.target.value) || 1))}
              inputProps={{ min: 1, max: 20 }}
            />
            <TextField
              label="Columns"
              type="number"
              value={tableCols}
              onChange={(e) => setTableCols(Math.max(1, parseInt(e.target.value) || 1))}
              inputProps={{ min: 1, max: 10 }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTableDialogOpen(false)}>Cancel</Button>
          <Button onClick={confirmTable} variant="contained">Insert Table</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ProfessionalRichTextEditor;