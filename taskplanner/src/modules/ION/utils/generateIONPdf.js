import jsPDF from "jspdf";
import html2canvas from "html2canvas";

/**
 * Build the ION print HTML for the given ionData + enclosures.
 *
 * ionData fields used:
 *   IONNumber, IONDate (ISO string), Subject, CommunicationReference (\n separated),
 *   IONBody (HTML), ToAddress, CopyTo, PreparedBy, PreparedByName, PreparedByDesignation,
 *   SentThrough, SentThroughName, SentThroughDesignation
 *
 * enclosures: array of { EnclosureDescription }
 */
const buildIONHtml = (ionData, enclosures = [], options = {}) => {
  const { isDraft = false } = options;

  // Format date as dd/mm/yyyy. IONDate may be an ISO datetime or just YYYY-MM-DD.
  const rawDate = (ionData.IONDate || "").toString();
  let formattedDate = "";
  if (rawDate) {
    const datePart = rawDate.includes("T") ? rawDate.split("T")[0] : rawDate;
    const parts = datePart.split("-");
    if (parts.length === 3) {
      formattedDate = `${parts[2]}/${parts[1]}/${parts[0]}`;
    } else {
      formattedDate = datePart;
    }
  }

  const logoUrl = `${window.location.origin}/assets/img/STFE_Logo.jpg`;

  const enclosuresBlock =
    enclosures.length > 0
      ? `<p style="margin: 4px 0;"><strong>Encl: ${enclosures
          .map((e, i) => `${i + 1}. ${e.EnclosureDescription}`)
          .join("<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;")}</strong></p>`
      : "";

  const refsBlock = ionData.CommunicationReference
    ? (() => {
        const refs = ionData.CommunicationReference.split("\n")
          .map((r) => r.trim())
          .filter((r) => r);
        if (refs.length === 0) return "";
        return `<div style="padding: 6px 12px;">
          <strong>Ref: ${refs
            .map((ref, i) => `${i + 1}. ${ref}`)
            .join("<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;")}</strong>
        </div>`;
      })()
    : "";

  const forwardedBlock =
    ionData.SentThrough && ionData.SentThrough !== ionData.PreparedBy
      ? `
        <div style="text-align: left; padding: 10px 12px 20px 12px;">
          <div style="margin-bottom: 8px;">Forwarded</div>
          <div style="margin-bottom: 30px;">&nbsp;</div>
          <div>${ionData.SentThroughName || ""}</div>
          ${ionData.SentThroughDesignation ? `<div style="font-size: 10pt;">${ionData.SentThroughDesignation}</div>` : ""}
        </div>`
      : "";

  const copyToBlock = ionData.CopyTo
    ? (() => {
        const lines = ionData.CopyTo.split("\n").map((l) => l.trim()).filter((l) => l);
        if (lines.length === 0) return "";
        return `
        <div style="padding: 8px 12px;">
          <div><strong>Copy To:</strong></div>
          <div>${lines.map((l, i) => `${i + 1}. ${l}`).join("<br/>")}</div>
        </div>`;
      })()
    : "";

  return `
    <div style="max-width: 100%; margin: 0 auto; position: relative; font-family: Arial, sans-serif; font-size: 11pt; color: black;">

      ${
        isDraft
          ? `
      <div style="
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%) rotate(-45deg);
        font-size: 100pt;
        font-weight: bold;
        color: rgba(255, 0, 0, 0.15);
        z-index: 1000;
        pointer-events: none;
        white-space: nowrap;
      ">DRAFT</div>
      `
          : ""
      }

      <!-- Boxed header: first 3 rows only -->
      <div style="border: 0.5px solid #ccc; margin-bottom: 16px; font-family: 'Georgia', serif;">

        <!-- Row 1: Office name + Logo -->
        <div style="display: flex; align-items: stretch; height: 65px; box-sizing: border-box;">
          <div style="
            flex: 1;
            display: flex;
            align-items: center;
            padding: 0 24px;
            font-size: 13pt;
            font-weight: bold;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            color: #222;
            box-sizing: border-box;
          ">
            OFFICE OF THE PROJECT DIRECTOR - STFE
          </div>
          <div style="width: 1px; background-color: #ccc; flex-shrink: 0;"></div>
          <div style="
            width: 140px;
            flex-shrink: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            box-sizing: border-box;
          ">
            <img src="${logoUrl}" style="max-height: 60px; max-width: 130px; width: auto; height: auto; object-fit: contain; padding: 2px;" crossorigin="anonymous" />
          </div>
        </div>

        <!-- Row 2: Title -->
        <div style="
          text-align: center;
          border-top: 0.5px solid #ccc;
          border-bottom: 0.5px solid #ccc;
          padding: 7px 0;
          font-size: 13pt;
          font-weight: bold;
          font-family: 'Georgia', serif;
          text-transform: uppercase;
          letter-spacing: 1px;
          color: #222;
          box-sizing: border-box;
        ">
          INTER OFFICE NOTE
        </div>

        <!-- Row 3: NO and DATE -->
        <div style="display: flex; align-items: stretch; height: 30px; box-sizing: border-box; font-family: Arial, sans-serif; font-size: 10.5pt; font-weight: bold; color: #222;">
          <div style="
            flex: 1;
            display: flex;
            align-items: center;
            padding: 0 24px;
            box-sizing: border-box;
            white-space: nowrap;
          ">
            NO: ${ionData.IONNumber || ""}
          </div>
          <div style="width: 1px; background-color: #ccc; flex-shrink: 0;"></div>
          <div style="
            width: 140px;
            flex-shrink: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 0 12px;
            box-sizing: border-box;
            white-space: nowrap;
          ">
            DATE: ${formattedDate}
          </div>
        </div>
      </div>

      <!-- Subject -->
      <div style="padding: 6px 12px;"><strong>Sub:  ${ionData.Subject || ""}</strong></div>

      ${refsBlock}

      <!-- ION Body -->
      <div style="padding: 10px 12px;">
        ${ionData.IONBody || ""}
      </div>

      <!-- Enclosures -->
      <div style="padding: 0px 12px;">
        ${enclosuresBlock}
      </div>

      <!-- Signature row -->
      <div style="display: flex; padding: 20px 12px 10px 12px;">
        <div style="margin-left: auto; text-align: center; min-width: 160px;">
          <div style="margin-bottom: 30px;">&nbsp;</div>
          <div>${ionData.PreparedByName || ""}</div>
          ${ionData.PreparedByDesignation ? `<div style="font-size: 10pt;">${ionData.PreparedByDesignation}</div>` : ""}
        </div>
      </div>

      ${forwardedBlock}

      <!-- To -->
      <div style="padding: 8px 12px; margin-top: 10px;">
        <div><strong>To,</strong></div>
        <div style="white-space: pre-line;">${ionData.ToAddress || ""}</div>
      </div>

      ${copyToBlock}

      ${
        isDraft
          ? `<p style="margin: 5px 0 0 0; color: #dc3545; font-weight: bold; text-align:center;">DRAFT - NOT FOR OFFICIAL USE</p>`
          : ""
      }
    </div>
  `;
};

/**
 * Generate a PDF blob URL for an ION note. Returns a Promise<string> (blob URL).
 *
 * Caller is responsible for calling URL.revokeObjectURL on the returned URL when done.
 */
export const generateIONPdf = async (ionData, enclosures = [], options = {}) => {
  const { isDraft = false } = options;

  // Page margins in mm
  const marginLeft = 15;
  const marginRight = 15;
  const marginTop = 15;
  const marginBottom = 15;

  // Content width = A4 width minus margins
  const contentWidthMM = 210 - marginLeft - marginRight; // 180mm

  // Hidden container for rendering
  const container = document.createElement("div");
  container.style.position = "absolute";
  container.style.left = "-9999px";
  container.style.width = `${contentWidthMM}mm`;
  container.style.backgroundColor = "white";
  container.style.padding = "0";
  container.style.fontFamily = "Arial, sans-serif";
  container.style.fontSize = "12pt";
  container.style.color = "black";

  container.innerHTML = buildIONHtml(ionData, enclosures, { isDraft });

  document.body.appendChild(container);

  try {
    const canvas = await html2canvas(container, {
      scale: 2,
      useCORS: true,
      logging: false,
      backgroundColor: "#ffffff",
      width: container.offsetWidth,
      height: container.offsetHeight,
    });

    const imgData = canvas.toDataURL("image/png");
    const pdf = new jsPDF("p", "mm", "a4");

    pdf.setProperties({
      title: ionData.IONNumber || "ION",
      subject: ionData.Subject || "",
      author: ionData.PreparedByName || "",
      creator: "SHRAM V2",
    });

    const pageWidth = pdf.internal.pageSize.getWidth();   // 210mm
    const pageHeight = pdf.internal.pageSize.getHeight(); // 297mm
    const imgWidth = canvas.width;
    const imgHeight = canvas.height;

    const printableWidth = pageWidth - marginLeft - marginRight;   // 180mm
    const printableHeight = pageHeight - marginTop - marginBottom; // 267mm

    const ratio = printableWidth / imgWidth;
    const scaledHeight = imgHeight * ratio;

    if (scaledHeight <= printableHeight) {
      pdf.addImage(imgData, "PNG", marginLeft, marginTop, printableWidth, scaledHeight);
    } else {
      const sourcePageHeight = printableHeight / ratio;
      let yOffset = 0;
      let pageNum = 0;

      while (yOffset < imgHeight) {
        if (pageNum > 0) pdf.addPage();

        const sliceHeight = Math.min(sourcePageHeight, imgHeight - yOffset);
        const pageCanvas = document.createElement("canvas");
        pageCanvas.width = imgWidth;
        pageCanvas.height = sliceHeight;
        const ctx = pageCanvas.getContext("2d");
        ctx.drawImage(canvas, 0, yOffset, imgWidth, sliceHeight, 0, 0, imgWidth, sliceHeight);

        const pageImgData = pageCanvas.toDataURL("image/png");
        const sliceScaledHeight = sliceHeight * ratio;

        pdf.addImage(pageImgData, "PNG", marginLeft, marginTop, printableWidth, sliceScaledHeight);

        yOffset += sourcePageHeight;
        pageNum++;
      }
    }

    // "Page X of Y" footer
    const totalPages = pdf.internal.getNumberOfPages();
    for (let i = 1; i <= totalPages; i++) {
      pdf.setPage(i);
      pdf.setFont("helvetica", "normal");
      pdf.setFontSize(9);
      pdf.setTextColor(100, 100, 100);
      const footerText = `Page ${i} of ${totalPages}`;
      const textWidth = pdf.getTextWidth(footerText);
      const footerX = (pageWidth - textWidth) / 2;
      const footerY = pageHeight - (marginBottom / 2);
      pdf.text(footerText, footerX, footerY);
    }

    const pdfBlob = pdf.output("blob");
    return URL.createObjectURL(pdfBlob);
  } finally {
    if (container.parentNode) {
      document.body.removeChild(container);
    }
  }
};

export default generateIONPdf;
