window.bncSummaryGenerator = {
    loadImage: function (src) {
        return new Promise((resolve) => {
            if (!src) {
                resolve(null);
                return;
            }
            const img = new Image();
            img.crossOrigin = "anonymous";
            img.onload = () => resolve(img);
            img.onerror = () => resolve(null);
            img.src = src;
        });
    },

    wrapText: function (ctx, text, maxWidth) {
        if (!text) return [""];
        const paragraphs = String(text).split(/\r?\n/);
        const lines = [];

        for (const para of paragraphs) {
            const words = para.trim().split(/\s+/);
            if (words.length === 0 || (words.length === 1 && words[0] === "")) {
                continue;
            }
            let currentLine = "";

            for (let i = 0; i < words.length; i++) {
                let word = words[i];

                // Si una sola palabra (ej. una URL larga o texto sin espacios) excede el ancho máximo, partirla por caracteres
                if (ctx.measureText(word).width > maxWidth) {
                    if (currentLine) {
                        lines.push(currentLine);
                        currentLine = "";
                    }
                    let chunk = "";
                    for (const ch of word) {
                        if (ctx.measureText(chunk + ch).width > maxWidth && chunk.length > 0) {
                            lines.push(chunk);
                            chunk = ch;
                        } else {
                            chunk += ch;
                        }
                    }
                    currentLine = chunk;
                } else {
                    const testLine = currentLine ? currentLine + " " + word : word;
                    if (ctx.measureText(testLine).width > maxWidth && currentLine) {
                        lines.push(currentLine);
                        currentLine = word;
                    } else {
                        currentLine = testLine;
                    }
                }
            }
            if (currentLine) {
                lines.push(currentLine);
            }
        }
        return lines.length > 0 ? lines : [""];
    },

    roundRect: function (ctx, x, y, w, h, r) {
        const radius = Math.min(r, w / 2, h / 2);
        ctx.beginPath();
        ctx.moveTo(x + radius, y);
        ctx.arcTo(x + w, y, x + w, y + h, radius);
        ctx.arcTo(x + w, y + h, x, y + h, radius);
        ctx.arcTo(x, y + h, x, y, radius);
        ctx.arcTo(x, y, x + w, y, radius);
        ctx.closePath();
    },

    downloadPersonSummary: async function (data) {
        // 1. Cargar imágenes necesarias (Logo BNC, Foto de Perfil, Imagen de Regalo)
        const [logoImg, profileImg, giftImg] = await Promise.all([
            this.loadImage("/images/BNC_Logo.png"),
            this.loadImage(data.profilePhotoUrl),
            this.loadImage(data.giftImageUrl)
        ]);

        const likes = Array.isArray(data.likes) && data.likes.length > 0 ? data.likes : ["Sin gustos registrados aún"];
        const dislikes = Array.isArray(data.dislikes) && data.dislikes.length > 0 ? data.dislikes : ["Sin restricciones registradas"];
        const allergies = Array.isArray(data.allergies) && data.allergies.length > 0 ? data.allergies : ["Ninguna alergia registrada"];

        const maxCount = Math.max(likes.length, dislikes.length, allergies.length);
        const totalItems = likes.length + dislikes.length + allergies.length;

        // 2. Determinar ancho del lienzo y número de columnas según la cantidad de ítems (soporta desde 1 hasta >1000 ítems)
        let canvasWidth = 900;
        let defaultCols = 1;
        let fontSize = 15;
        let lineHeight = 21;

        if (maxCount > 250 || totalItems > 500) {
            canvasWidth = 1900;
            defaultCols = 5;
            fontSize = 13;
            lineHeight = 18;
        } else if (maxCount > 80 || totalItems > 160) {
            canvasWidth = 1600;
            defaultCols = 4;
            fontSize = 13.5;
            lineHeight = 19;
        } else if (maxCount > 25 || totalItems > 50) {
            canvasWidth = 1320;
            defaultCols = 3;
            fontSize = 14;
            lineHeight = 20;
        } else if (maxCount > 8 || totalItems > 16) {
            canvasWidth = 1040;
            defaultCols = 2;
            fontSize = 14.5;
            lineHeight = 20;
        }

        const measureCanvas = document.createElement("canvas");
        const mCtx = measureCanvas.getContext("2d");

        const outerPad = 36;
        const contentW = canvasWidth - outerPad * 2;
        const sectionPad = 22;
        const innerSectionW = contentW - sectionPad * 2;
        const colGap = 12;
        const rowGap = 10;

        // Función auxiliar para precalcular el layout de una sección de ítems (Gustos, Disgustos, Alergias)
        const measureSection = (items, iconPrefix) => {
            const cols = items.length <= 3 ? 1 : Math.min(defaultCols, items.length);
            const pillW = (innerSectionW - colGap * (cols - 1)) / cols;
            const textMaxW = pillW - 40; // espacio para el icono y padding interno

            mCtx.font = `500 ${fontSize}px 'Segoe UI', Roboto, Arial, sans-serif`;
            const measuredItems = items.map((item) => {
                const wrapped = this.wrapText(mCtx, item, textMaxW);
                const pillH = Math.max(38, wrapped.length * lineHeight + 18);
                return { lines: wrapped, height: pillH, icon: iconPrefix };
            });

            const rows = [];
            for (let i = 0; i < measuredItems.length; i += cols) {
                const rowSlice = measuredItems.slice(i, i + cols);
                const rowH = Math.max(...rowSlice.map((r) => r.height));
                rows.push({ items: rowSlice, height: rowH, cols, pillW });
            }

            const totalRowsH = rows.reduce((acc, r) => acc + r.height, 0) + Math.max(0, rows.length - 1) * rowGap;
            const blockH = 54 + totalRowsH + sectionPad;
            return { rows, blockH };
        };

        const likesLayout = measureSection(likes, "✓");
        const dislikesLayout = measureSection(dislikes, "✕");
        const allergiesLayout = measureSection(allergies, "!");

        // Precalcular altura del bloque de Regalo Deseado (si tiene imagen o URL)
        const hasGift = Boolean(giftImg || data.giftReferenceUrl);
        let giftBlockH = 0;
        let giftUrlLines = [];
        let giftImgDrawW = 0;
        let giftImgDrawH = 0;

        if (hasGift) {
            giftBlockH = 58; // título del bloque
            if (giftImg) {
                const maxImgW = Math.min(360, innerSectionW - 20);
                const maxImgH = 220;
                const ratio = Math.min(maxImgW / giftImg.width, maxImgH / giftImg.height, 1);
                giftImgDrawW = Math.round(giftImg.width * ratio);
                giftImgDrawH = Math.round(giftImg.height * ratio);
                giftBlockH += giftImgDrawH + 24;
            }
            if (data.giftReferenceUrl) {
                mCtx.font = `600 13.5px 'Segoe UI', Roboto, Arial, sans-serif`;
                giftUrlLines = this.wrapText(mCtx, "Enlace de referencia: " + data.giftReferenceUrl, innerSectionW - 24);
                giftBlockH += giftUrlLines.length * 19 + 22;
            }
            giftBlockH += 14;
        }

        // Precalcular altura de Tallas / Color Favorito
        const quickBadges = [];
        if (data.favoriteColor) quickBadges.push({ label: "COLOR FAVORITO", val: data.favoriteColor });
        if (data.shirtSize) quickBadges.push({ label: "TALLA CAMISA", val: data.shirtSize });
        if (data.shoeSize) quickBadges.push({ label: "CALZADO", val: data.shoeSize });
        const quickBarH = quickBadges.length > 0 ? 74 : 0;

        // Precalcular altura de Notas Extras
        let notesBlockH = 0;
        let notesLines = [];
        if (data.notes && String(data.notes).trim().length > 0) {
            mCtx.font = `italic 14.5px 'Segoe UI', Roboto, Arial, sans-serif`;
            notesLines = this.wrapText(mCtx, `"${String(data.notes).trim()}"`, innerSectionW - 20);
            notesBlockH = 48 + notesLines.length * 21 + 20;
        }

        const headerH = 148;
        const footerH = 56;
        const gapBetweenBlocks = 18;

        let totalHeight = outerPad + headerH + 22;
        if (quickBarH > 0) totalHeight += quickBarH + gapBetweenBlocks;
        if (hasGift) totalHeight += giftBlockH + gapBetweenBlocks;
        totalHeight += likesLayout.blockH + gapBetweenBlocks;
        totalHeight += dislikesLayout.blockH + gapBetweenBlocks;
        totalHeight += allergiesLayout.blockH + gapBetweenBlocks;
        if (notesBlockH > 0) totalHeight += notesBlockH + gapBetweenBlocks;
        totalHeight += footerH + outerPad;

        // Ajustar escala Retina asegurando no superar el límite máximo de textura del navegador (16384px)
        const maxSide = Math.max(canvasWidth, totalHeight);
        let scale = 2;
        if (maxSide * scale > 15500) {
            scale = Math.max(1, 15500 / maxSide);
        }

        const canvas = document.createElement("canvas");
        canvas.width = Math.round(canvasWidth * scale);
        canvas.height = Math.round(totalHeight * scale);
        const ctx = canvas.getContext("2d");
        ctx.scale(scale, scale);

        // 3. Fondo general
        ctx.fillStyle = "#f4f7fb";
        ctx.fillRect(0, 0, canvasWidth, totalHeight);

        // Contenedor tarjeta principal blanca
        this.roundRect(ctx, 16, 16, canvasWidth - 32, totalHeight - 32, 22);
        ctx.fillStyle = "#ffffff";
        ctx.fill();
        ctx.lineWidth = 1.5;
        ctx.strokeStyle = "#e2e8f0";
        ctx.stroke();

        // 4. Encabezado Corporativo BNC (Degradado Azul Marino + Acento Naranja)
        let curY = 16;
        ctx.save();
        this.roundRect(ctx, 16, curY, canvasWidth - 32, headerH, 22);
        ctx.clip();

        const headerGrad = ctx.createLinearGradient(16, curY, canvasWidth - 16, curY + headerH);
        headerGrad.addColorStop(0, "#001428");
        headerGrad.addColorStop(0.6, "#002855");
        headerGrad.addColorStop(1, "#003b7a");
        ctx.fillStyle = headerGrad;
        ctx.fillRect(16, curY, canvasWidth - 32, headerH);

        // Barra inferior naranja del header
        ctx.fillStyle = "#FF6600";
        ctx.fillRect(16, curY + headerH - 6, canvasWidth - 32, 6);

        // Avatar o Foto de Perfil circular
        const avatarX = outerPad + 14;
        const avatarY = curY + 28;
        const avatarSize = 86;

        ctx.save();
        ctx.beginPath();
        ctx.arc(avatarX + avatarSize / 2, avatarY + avatarSize / 2, avatarSize / 2, 0, Math.PI * 2);
        ctx.closePath();
        ctx.clip();

        if (profileImg) {
            // Dibujar foto de perfil centrada (cover)
            const sRatio = Math.max(avatarSize / profileImg.width, avatarSize / profileImg.height);
            const sw = profileImg.width * sRatio;
            const sh = profileImg.height * sRatio;
            const sx = avatarX + (avatarSize - sw) / 2;
            const sy = avatarY + (avatarSize - sh) / 2;
            ctx.drawImage(profileImg, sx, sy, sw, sh);
        } else {
            const avGrad = ctx.createLinearGradient(avatarX, avatarY, avatarX + avatarSize, avatarY + avatarSize);
            avGrad.addColorStop(0, data.avatarColor || "#002855");
            avGrad.addColorStop(1, "#FF6600");
            ctx.fillStyle = avGrad;
            ctx.fillRect(avatarX, avatarY, avatarSize, avatarSize);

            ctx.fillStyle = "#ffffff";
            ctx.font = "bold 30px 'Segoe UI', Roboto, Arial, sans-serif";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText(data.initials || "BNC", avatarX + avatarSize / 2, avatarY + avatarSize / 2);
        }
        ctx.restore();

        // Borde circular naranja del avatar
        ctx.beginPath();
        ctx.arc(avatarX + avatarSize / 2, avatarY + avatarSize / 2, avatarSize / 2 + 1.5, 0, Math.PI * 2);
        ctx.lineWidth = 3;
        ctx.strokeStyle = "#FF6600";
        ctx.stroke();

        // Nombre del participante y subtítulo
        const titleX = avatarX + avatarSize + 22;
        ctx.textAlign = "left";
        ctx.textBaseline = "top";

        // Pill superior
        this.roundRect(ctx, titleX, curY + 32, 235, 24, 12);
        ctx.fillStyle = "rgba(255, 102, 0, 0.9)";
        ctx.fill();
        ctx.fillStyle = "#ffffff";
        ctx.font = "bold 11.5px 'Segoe UI', Roboto, Arial, sans-serif";
        ctx.fillText("AMIGO SECRETO BNC 2026 • FICHA OFICIAL", titleX + 12, curY + 38);

        // Nombre completo
        ctx.fillStyle = "#ffffff";
        ctx.font = "bold 30px 'Segoe UI', Roboto, Arial, sans-serif";
        ctx.fillText(data.fullName || "Participante BNC", titleX, curY + 64);

        ctx.fillStyle = "rgba(255, 255, 255, 0.78)";
        ctx.font = "500 14px 'Segoe UI', Roboto, Arial, sans-serif";
        ctx.fillText("Resumen de gustos, preferencias y restricciones", titleX, curY + 102);

        // Logo BNC en la esquina derecha del header
        if (logoImg) {
            const logoBoxSize = 66;
            const logoBoxX = canvasWidth - outerPad - logoBoxSize - 8;
            const logoBoxY = curY + 36;
            this.roundRect(ctx, logoBoxX, logoBoxY, logoBoxSize, logoBoxSize, 14);
            ctx.fillStyle = "#ffffff";
            ctx.fill();
            ctx.drawImage(logoImg, logoBoxX + 7, logoBoxY + 7, logoBoxSize - 14, logoBoxSize - 14);
        }

        ctx.restore();
        curY += headerH + 22;

        // 5. Barra Rápida de Tallas y Color Favorito
        if (quickBadges.length > 0) {
            const badgeW = (contentW - (quickBadges.length - 1) * 14) / quickBadges.length;
            quickBadges.forEach((b, idx) => {
                const bx = outerPad + idx * (badgeW + 14);
                this.roundRect(ctx, bx, curY, badgeW, quickBarH, 12);
                ctx.fillStyle = "#f8fafc";
                ctx.fill();
                ctx.lineWidth = 1;
                ctx.strokeStyle = "#cbd5e1";
                ctx.stroke();

                ctx.textAlign = "center";
                ctx.fillStyle = "#64748b";
                ctx.font = "bold 11px 'Segoe UI', Roboto, Arial, sans-serif";
                ctx.fillText(b.label, bx + badgeW / 2, curY + 16);

                ctx.fillStyle = "#002855";
                ctx.font = "bold 17px 'Segoe UI', Roboto, Arial, sans-serif";
                ctx.fillText(b.val, bx + badgeW / 2, curY + 38);
            });
            ctx.textAlign = "left";
            curY += quickBarH + gapBetweenBlocks;
        }

        // 6. Bloque de Regalo Deseado (Imagen + URL Referencial)
        if (hasGift) {
            this.roundRect(ctx, outerPad, curY, contentW, giftBlockH, 14);
            ctx.fillStyle = "#fff7ed";
            ctx.fill();
            ctx.lineWidth = 1.5;
            ctx.strokeStyle = "#fdba74";
            ctx.stroke();

            ctx.fillStyle = "#002855";
            ctx.font = "bold 17px 'Segoe UI', Roboto, Arial, sans-serif";
            ctx.fillText("Regalo Deseado / Referencia Visual", outerPad + sectionPad, curY + 18);

            let innerGiftY = curY + 52;
            if (giftImg) {
                const imgX = outerPad + (contentW - giftImgDrawW) / 2;
                this.roundRect(ctx, imgX - 8, innerGiftY - 6, giftImgDrawW + 16, giftImgDrawH + 12, 10);
                ctx.fillStyle = "#ffffff";
                ctx.fill();
                ctx.strokeStyle = "#e2e8f0";
                ctx.lineWidth = 1;
                ctx.stroke();

                ctx.drawImage(giftImg, imgX, innerGiftY, giftImgDrawW, giftImgDrawH);
                innerGiftY += giftImgDrawH + 18;
            }

            if (giftUrlLines.length > 0) {
                this.roundRect(ctx, outerPad + sectionPad, innerGiftY, innerSectionW, giftUrlLines.length * 19 + 14, 8);
                ctx.fillStyle = "#ffffff";
                ctx.fill();
                ctx.strokeStyle = "#fed7aa";
                ctx.stroke();

                ctx.fillStyle = "#c2410c";
                ctx.font = "600 13.5px 'Segoe UI', Roboto, Arial, sans-serif";
                giftUrlLines.forEach((line, lIdx) => {
                    ctx.fillText(line, outerPad + sectionPad + 12, innerGiftY + 8 + lIdx * 19);
                });
            }

            curY += giftBlockH + gapBetweenBlocks;
        }

        // Función auxiliar para dibujar cada sección de ítems (Gustos, Disgustos, Alergias)
        const drawSection = (title, countBadge, layout, theme) => {
            this.roundRect(ctx, outerPad, curY, contentW, layout.blockH, 14);
            ctx.fillStyle = theme.bg;
            ctx.fill();
            ctx.lineWidth = 1.5;
            ctx.strokeStyle = theme.border;
            ctx.stroke();

            // Título de sección
            ctx.fillStyle = theme.titleColor;
            ctx.font = "bold 17px 'Segoe UI', Roboto, Arial, sans-serif";
            ctx.fillText(title, outerPad + sectionPad, curY + 18);

            // Contador a la derecha
            ctx.textAlign = "right";
            ctx.font = "bold 12.5px 'Segoe UI', Roboto, Arial, sans-serif";
            ctx.fillStyle = theme.titleColor;
            ctx.fillText(countBadge, outerPad + contentW - sectionPad, curY + 20);
            ctx.textAlign = "left";

            let rowY = curY + 52;
            for (const row of layout.rows) {
                row.items.forEach((item, colIdx) => {
                    const pillX = outerPad + sectionPad + colIdx * (row.pillW + colGap);
                    this.roundRect(ctx, pillX, rowY, row.pillW, row.height, 8);
                    ctx.fillStyle = "#ffffff";
                    ctx.fill();
                    ctx.lineWidth = 1;
                    ctx.strokeStyle = theme.pillBorder;
                    ctx.stroke();

                    // Círculo icono
                    ctx.beginPath();
                    ctx.arc(pillX + 18, rowY + 19, 9, 0, Math.PI * 2);
                    ctx.fillStyle = theme.iconBg;
                    ctx.fill();

                    ctx.fillStyle = "#ffffff";
                    ctx.font = "bold 11px 'Segoe UI', Arial, sans-serif";
                    ctx.textAlign = "center";
                    ctx.textBaseline = "middle";
                    ctx.fillText(item.icon, pillX + 18, rowY + 19);

                    // Líneas de texto del ítem
                    ctx.textAlign = "left";
                    ctx.textBaseline = "top";
                    ctx.fillStyle = "#0f172a";
                    ctx.font = `500 ${fontSize}px 'Segoe UI', Roboto, Arial, sans-serif`;
                    item.lines.forEach((line, idx) => {
                        ctx.fillText(line, pillX + 34, rowY + 10 + idx * lineHeight);
                    });
                });
                rowY += row.height + rowGap;
            }

            curY += layout.blockH + gapBetweenBlocks;
        };

        // 7. Dibujar Cosas que le gustan
        drawSection(
            "Cosas que le gustan / Ideas de regalo",
            `${likes.length} ${likes.length === 1 ? "ítem" : "ítems"}`,
            likesLayout,
            {
                bg: "#f0fdf4",
                border: "#bbf7d0",
                pillBorder: "#dcfce7",
                titleColor: "#15803d",
                iconBg: "#16a34a"
            }
        );

        // 8. Dibujar Cosas que NO le gustan
        drawSection(
            "Cosas que NO le gustan (Evitar)",
            `${dislikes.length} ${dislikes.length === 1 ? "ítem" : "ítems"}`,
            dislikesLayout,
            {
                bg: "#fff1f2",
                border: "#fecdd3",
                pillBorder: "#ffe4e6",
                titleColor: "#be123c",
                iconBg: "#e11d48"
            }
        );

        // 9. Dibujar Alergias y Restricciones
        drawSection(
            "Alergias y Restricciones Médicas",
            `${allergies.length} ${allergies.length === 1 ? "registro" : "registros"}`,
            allergiesLayout,
            {
                bg: "#fffbeb",
                border: "#fde68a",
                pillBorder: "#fef3c7",
                titleColor: "#b45309",
                iconBg: "#d97706"
            }
        );

        // 10. Dibujar Notas Adicionales
        if (notesBlockH > 0) {
            this.roundRect(ctx, outerPad, curY, contentW, notesBlockH, 12);
            ctx.fillStyle = "#f8fafc";
            ctx.fill();
            ctx.lineWidth = 1;
            ctx.strokeStyle = "#cbd5e1";
            ctx.stroke();

            ctx.fillStyle = "#475569";
            ctx.font = "bold 12.5px 'Segoe UI', Roboto, Arial, sans-serif";
            ctx.fillText("MENSAJE / NOTAS PARA EL AMIGO SECRETO:", outerPad + sectionPad, curY + 16);

            ctx.fillStyle = "#0f172a";
            ctx.font = "italic 14.5px 'Segoe UI', Roboto, Arial, sans-serif";
            notesLines.forEach((line, idx) => {
                ctx.fillText(line, outerPad + sectionPad, curY + 40 + idx * 21);
            });

            curY += notesBlockH + gapBetweenBlocks;
        }

        // 11. Pie de página corporativo BNC
        ctx.fillStyle = "#64748b";
        ctx.font = "600 12.5px 'Segoe UI', Roboto, Arial, sans-serif";
        ctx.textAlign = "center";
        ctx.fillText(
            "Banco Nacional de Crédito (BNC) • Portal Interactivo Amigo Secreto 2026",
            canvasWidth / 2,
            curY + 12
        );

        // 12. Descargar como archivo .png
        const safeFileName = (data.fullName || "Participante")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .replace(/[^a-zA-Z0-9_-]/g, "_");

        return new Promise((resolve) => {
            canvas.toBlob((blob) => {
                if (!blob) {
                    resolve(false);
                    return;
                }
                const url = URL.createObjectURL(blob);
                const link = document.createElement("a");
                link.href = url;
                link.download = `Resumen_AmigoSecreto_${safeFileName}.png`;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                setTimeout(() => URL.revokeObjectURL(url), 5000);
                resolve(true);
            }, "image/png");
        });
    }
};
