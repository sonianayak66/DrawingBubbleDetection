import React, { useRef, useEffect } from 'react';
import {
  Box,
  Typography,
  ButtonGroup,
  IconButton,
  Divider,
  Paper,
} from '@mui/material';
import {
  FormatBold,
  FormatItalic,
  FormatUnderlined,
  FormatListBulleted,
  FormatListNumbered,
  FormatAlignLeft,
  FormatAlignCenter,
  FormatAlignRight,
} from '@mui/icons-material';

const SimpleRichTextEditor = ({ 
  value, 
  onChange, 
  placeholder = "Enter content...",
  disabled = false,
  label = "Content"
}) => {
  const editorRef = useRef(null);

  useEffect(() => {
    if (editorRef.current && value !== editorRef.current.innerHTML) {
      editorRef.current.innerHTML = value || '';
    }
  }, [value]);

  const executeCommand = (command, value = null) => {
    document.execCommand(command, false, value);
    handleContentChange();
  };

  const handleContentChange = () => {
    if (editorRef.current && onChange) {
      onChange(editorRef.current.innerHTML);
    }
  };

  return (
    <Box>
      <Typography variant="subtitle2" gutterBottom>
        {label} *
      </Typography>
      
      {/* Toolbar */}
      {!disabled && (
        <Paper sx={{ p: 1, mb: 1, display: 'flex', flexWrap: 'wrap', gap: 1 }}>
          <ButtonGroup size="small">
            <IconButton onClick={() => executeCommand('bold')} size="small">
              <FormatBold />
            </IconButton>
            <IconButton onClick={() => executeCommand('italic')} size="small">
              <FormatItalic />
            </IconButton>
            <IconButton onClick={() => executeCommand('underline')} size="small">
              <FormatUnderlined />
            </IconButton>
          </ButtonGroup>
          
          <Divider orientation="vertical" flexItem />
          
          <ButtonGroup size="small">
            <IconButton onClick={() => executeCommand('insertUnorderedList')} size="small">
              <FormatListBulleted />
            </IconButton>
            <IconButton onClick={() => executeCommand('insertOrderedList')} size="small">
              <FormatListNumbered />
            </IconButton>
          </ButtonGroup>
          
          <Divider orientation="vertical" flexItem />
          
          <ButtonGroup size="small">
            <IconButton onClick={() => executeCommand('justifyLeft')} size="small">
              <FormatAlignLeft />
            </IconButton>
            <IconButton onClick={() => executeCommand('justifyCenter')} size="small">
              <FormatAlignCenter />
            </IconButton>
            <IconButton onClick={() => executeCommand('justifyRight')} size="small">
              <FormatAlignRight />
            </IconButton>
          </ButtonGroup>
        </Paper>
      )}

      {/* Content Editable Area */}
      <Box
        ref={editorRef}
        contentEditable={!disabled}
        onInput={handleContentChange}
        onBlur={handleContentChange}
        sx={{
          border: '1px solid',
          borderColor: disabled ? 'action.disabled' : 'divider',
          borderRadius: 1,
          p: 2,
          minHeight: 300,
          outline: 'none',
          backgroundColor: disabled ? 'action.disabledBackground' : 'background.paper',
          '&:focus': {
            borderColor: 'primary.main',
            borderWidth: 2,
          },
          '&:empty:before': {
            content: `"${placeholder}"`,
            color: 'text.disabled',
            fontStyle: 'italic'
          }
        }}
        suppressContentEditableWarning={true}
      />
    </Box>
  );
};

export default SimpleRichTextEditor;