// JoditRichTextEditor.jsx
import React, { useRef, useMemo, useEffect } from 'react';
import JoditEditor from 'jodit-react';

const JoditRichTextEditor = ({
  value = '',
  onChange,
  placeholder = 'Start typing or paste content from MS Word…',
  preserveOfficeFormatting = true,
  disabled = false,
  height = 400
}) => {
  const editor = useRef(null);

  const ensureTableBorders = (html) => {
    try {
      const parser = new DOMParser();
      const doc = parser.parseFromString(html, 'text/html');

      doc.querySelectorAll('table').forEach((table) => {
        const tblStyle = table.getAttribute('style') || '';
        if (!/border-collapse\s*:/.test(tblStyle)) {
          table.style.borderCollapse = 'collapse';
        }

        if (table.classList.contains('MsoTableGrid')) {
          table.querySelectorAll('td, th').forEach((cell) => {
            const cs = cell.getAttribute('style') || '';
            const hasBorder = /border\s*:/.test(cs) || cell.getAttribute('border');
            if (!hasBorder) {
              cell.style.border = '1px solid #ddd';
            }
            if (!/padding\s*:/.test(cs)) {
              cell.style.padding = '8px';
            }
          });
        } else {
          const hasAnyCellBorder = Array.from(table.querySelectorAll('td, th'))
            .some((c) => /border\s*:/.test(c.getAttribute('style') || '') || c.getAttribute('border'));

          if (!hasAnyCellBorder) {
            table.querySelectorAll('td, th').forEach((cell) => {
              const cs = cell.getAttribute('style') || '';
              if (!/border\s*:/.test(cs) && !cell.getAttribute('border')) {
                cell.style.border = '1px solid #ddd';
              }
              if (!/padding\s*:/.test(cs)) {
                cell.style.padding = '8px';
              }
            });
          }
        }
      });

      return doc.body.innerHTML;
    } catch {
      return html;
    }
  };

  const config = useMemo(() => {
    const base = {
      readonly: disabled,
      placeholder: placeholder || 'Start typing...',
      height: height,
      // Dark mode support
      theme: 'default',
      // Enable dark mode detection
      style: {
        background: 'transparent',
        color: 'inherit',
        lineHeight: 1
      },
      defaultLineHeight: 1,
      // Toolbar configuration
      buttons: [
        'bold', 'italic', 'underline', 'strikethrough', '|',
        'font', 'fontsize', 'brush', '|',
        'align', 'lineHeight', 'indent', 'outdent', '|',
        'link', 'image', 'table', 'hr', '|',
        'undo', 'redo', '|',
        'eraser', 'source'
      ],
      toolbarAdaptive: false,
      toolbarSticky: false,
      showCharsCounter: false,
      showWordsCounter: false,
      showXPathInStatusbar: false,
    };

    if (!preserveOfficeFormatting) return base;

    return {
      ...base,
      // Keep Office HTML
      defaultActionOnPaste: 'insert_as_html',
      askBeforePasteHTML: false,
      askBeforePasteFromWord: false,
      processPasteHTML: false,
      cleanHTML: {
        cleanOnPaste: false,
        replaceNBSP: false,
        removeEmptyElements: false,
        fillEmptyParagraph: false
      },
      // Offline images
      uploader: {
        insertImageAsBase64URI: true,
        url: null
      },
      filebrowser: {
        ajax: { url: null }
      },
      // Paste handler: inject HTML, then repair table borders if needed
      events: {
        paste: function (e) {
          const evt = e || window.event;
          const cd =
            evt?.clipboardData ||
            evt?.originalEvent?.clipboardData ||
            window.clipboardData;

          if (!cd) return true;

          const html = cd.getData && cd.getData('text/html');
          if (html) {
            evt.preventDefault();

            const fixed = ensureTableBorders(html);

            if (this?.selection?.insertHTML) {
              this.selection.insertHTML(fixed);
            } else {
              this.value = (this.value || '') + fixed;
            }
            return false;
          }
          return true;
        }
      }
    };
  }, [placeholder, preserveOfficeFormatting, disabled, height]);

  return (
    <JoditEditor
      ref={editor}
      value={value || ''}
      config={config}
      tabIndex={1}
      onBlur={(newContent) => {
        if (onChange) {
          onChange(newContent);
        }
      }}
      onChange={(newContent) => {
        // Optional: you can also trigger onChange on every keystroke
        // For better performance, we're using onBlur instead
      }}
    />
  );
};

export default JoditRichTextEditor;
