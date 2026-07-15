import React, { useRef, useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Button,
  Toolbar,
  Divider,
} from '@mui/material';
import {
  Save,
  Image,
  TableChart,
  Link,
} from '@mui/icons-material';

// Import Quill and styles
import Quill from 'quill';
import 'quill/dist/quill.snow.css';

const QuillRichTextEditor = ({
  value = '',
  onChange,
  placeholder = "Start typing or paste content from MS Word...",
  disabled = false,
  label = "Content",
  height = 500
}) => {
  const quillRef = useRef(null);
  const [quill, setQuill] = useState(null);

  useEffect(() => {
    if (quillRef.current && !quill) {
      // Initialize Quill with corrected configuration
      const quillInstance = new Quill(quillRef.current, {
        theme: 'snow',
        placeholder: placeholder,
        readOnly: disabled,
        modules: {
          toolbar: {
            container: [
              // Text formatting
              [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
              [{ 'font': [] }],
              [{ 'size': ['small', false, 'large', 'huge'] }],
              
              // Text style
              ['bold', 'italic', 'underline', 'strike'],
              [{ 'color': [] }, { 'background': [] }],
              [{ 'script': 'sub'}, { 'script': 'super' }],
              
              // Lists and alignment
              [{ 'list': 'ordered'}, { 'list': 'bullet' }], // Corrected: 'bullet' is a value, not format
              [{ 'indent': '-1'}, { 'indent': '+1' }],
              [{ 'align': [] }],
              
              // Links, images, and media
              ['link', 'image', 'video'],
              
              // Block formatting
              ['blockquote', 'code-block'],
              
              // Clear formatting
              ['clean']
            ],
            handlers: {
              // Custom image handler
              image: function() {
                const input = document.createElement('input');
                input.setAttribute('type', 'file');
                input.setAttribute('accept', 'image/*');
                input.click();

                input.onchange = () => {
                  const file = input.files[0];
                  if (file) {
                    const reader = new FileReader();
                    reader.onload = (e) => {
                      const range = this.quill.getSelection(true);
                      this.quill.insertEmbed(range.index, 'image', e.target.result);
                      this.quill.setSelection(range.index + 1);
                    };
                    reader.readAsDataURL(file);
                  }
                };
              },
              
              // Custom link handler
              link: function(value) {
                if (value) {
                  const href = prompt('Enter link URL:');
                  this.quill.format('link', href);
                } else {
                  this.quill.format('link', false);
                }
              }
            }
          },
          history: {
            delay: 1000,
            maxStack: 500,
            userOnly: true
          },
          clipboard: {
            // Better MS Word paste handling
            matchVisual: false
          }
        },
        // Corrected formats array - only actual Quill formats
        formats: [
          'header', 'font', 'size',
          'bold', 'italic', 'underline', 'strike', 'blockquote',
          'list', 'indent', 'align', // 'list' is the format, 'bullet'/'ordered' are values
          'link', 'image', 'video',
          'color', 'background',
          'script',
          'code-block'
        ]
      });

      // Set initial content
      if (value) {
        quillInstance.clipboard.dangerouslyPasteHTML(value);
      }

      // Listen for text changes
      quillInstance.on('text-change', () => {
        const html = quillInstance.root.innerHTML;
        if (onChange) {
          onChange(html);
        }
      });

      // Handle image drops
      quillInstance.root.addEventListener('drop', (e) => {
        e.preventDefault();
        const files = Array.from(e.dataTransfer.files);
        const imageFiles = files.filter(file => file.type.startsWith('image/'));
        
        imageFiles.forEach(file => {
          const reader = new FileReader();
          reader.onload = (event) => {
            const range = quillInstance.getSelection(true);
            quillInstance.insertEmbed(range.index, 'image', event.target.result);
            quillInstance.setSelection(range.index + 1);
          };
          reader.readAsDataURL(file);
        });
      });

      // Prevent default drag behavior
      quillInstance.root.addEventListener('dragover', (e) => {
        e.preventDefault();
      });

      setQuill(quillInstance);
    }
  }, []);

  useEffect(() => {
    if (quill && value !== quill.root.innerHTML) {
      quill.clipboard.dangerouslyPasteHTML(value || '');
    }
  }, [value, quill]);

  // Custom table insertion function
  const insertTable = () => {
    if (!quill) return;
    
    const rows = prompt('Number of rows:', '3');
    const cols = prompt('Number of columns:', '3');
    
    if (rows && cols) {
      let tableHTML = '<table style="border-collapse: collapse; width: 100%; margin: 10px 0; border: 1px solid #ddd;">';
      
      // Create header row
      tableHTML += '<tr>';
      for (let j = 0; j < parseInt(cols); j++) {
        tableHTML += `<th style="padding: 8px; background-color: #f5f5f5; border: 1px solid #ddd; text-align: left;">Header ${j + 1}</th>`;
      }
      tableHTML += '</tr>';
      
      // Create data rows
      for (let i = 1; i < parseInt(rows); i++) {
        tableHTML += '<tr>';
        for (let j = 0; j < parseInt(cols); j++) {
          tableHTML += `<td style="padding: 8px; border: 1px solid #ddd;">Cell ${i},${j + 1}</td>`;
        }
        tableHTML += '</tr>';
      }
      
      tableHTML += '</table><p><br></p>';
      
      const range = quill.getSelection(true);
      quill.clipboard.dangerouslyPasteHTML(range.index, tableHTML);
    }
  };

  if (disabled && !quill) {
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

      {/* Additional custom toolbar */}
      <Paper sx={{ mb: 1 }}>
        <Toolbar variant="dense">
          <Button
            size="small"
            startIcon={<TableChart />}
            onClick={insertTable}
            disabled={disabled}
          >
            Insert Table
          </Button>
          <Divider orientation="vertical" flexItem sx={{ mx: 1 }} />
          <Typography variant="caption" color="text.secondary">
            Drag & drop images • Copy/paste from MS Word • Full formatting support
          </Typography>
        </Toolbar>
      </Paper>

      {/* Quill Editor Container */}
      <Box
        sx={{
          border: '1px solid',
          borderColor: 'divider',
          borderRadius: 1,
          '& .ql-editor': {
            minHeight: height,
            fontSize: '14px',
            lineHeight: 1.6,
            fontFamily: 'Arial, sans-serif',
          },
          '& .ql-toolbar': {
            borderBottom: '1px solid',
            borderColor: 'divider',
          },
          '& .ql-container': {
            fontSize: '14px',
          },
          // Custom styles for better MS Word compatibility
          '& .ql-editor p': {
            marginBottom: '10px',
          },
          '& .ql-editor h1, & .ql-editor h2, & .ql-editor h3': {
            marginTop: '15px',
            marginBottom: '10px',
          },
          '& .ql-editor img': {
            maxWidth: '100%',
            height: 'auto',
          },
          '& .ql-editor table': {
            borderCollapse: 'collapse',
            width: '100%',
            '& td, & th': {
              border: '1px solid #ddd',
              padding: '8px',
            },
            '& th': {
              backgroundColor: '#f5f5f5',
              fontWeight: 'bold',
            }
          },
          // Better list styling
          '& .ql-editor ul, & .ql-editor ol': {
            paddingLeft: '1.5em',
          },
          '& .ql-editor li': {
            marginBottom: '5px',
          }
        }}
      >
        <div ref={quillRef} />
      </Box>

      {/* Status bar */}
      <Box sx={{ 
        mt: 1, 
        p: 1, 
        backgroundColor: 'action.hover', 
        borderRadius: 1,
        fontSize: '12px',
        color: 'text.secondary'
      }}>
        Professional rich text editor with full MS Word compatibility • 100% offline • Drag & drop images
      </Box>
    </Box>
  );
};

export default QuillRichTextEditor;