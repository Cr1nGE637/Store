import { AlertTriangle, CheckCircle2, Download, FileSpreadsheet, Upload } from "lucide-react";
import { useQueryClient } from "@tanstack/react-query";
import { ChangeEvent, useMemo, useRef, useState } from "react";
import styled from "styled-components";
import { api } from "../../api/client";
import { queryKeys } from "../../api/queryKeys";
import type { ProductImportPreview, ProductImportPreviewRow, ProductImportResult } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { FormError, SoftBadge } from "../../components/ui/common";

type ProductImportExportPanelProps = {
  onChanged: () => Promise<unknown>;
};

export function ProductImportExportPanel({ onChanged }: ProductImportExportPanelProps) {
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<ProductImportPreview | null>(null);
  const [importResult, setImportResult] = useState<ProductImportResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isExporting, setIsExporting] = useState(false);
  const [isPreviewing, setIsPreviewing] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const inputRef = useRef<HTMLInputElement | null>(null);
  const queryClient = useQueryClient();

  const rows = useMemo(() => preview?.rows ?? [], [preview]);
  const canApplyImport = Boolean(file && preview && preview.errorCount === 0 && !isImporting && !isPreviewing);

  async function downloadExport() {
    setError(null);
    setIsExporting(true);

    try {
      const result = await api.downloadProductsExport();
      const url = URL.createObjectURL(result.blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = result.fileName ?? "storefit-products.xlsx";
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Export failed");
    } finally {
      setIsExporting(false);
    }
  }

  async function previewImport() {
    if (!file) {
      setError("Выберите Excel-файл");
      return;
    }

    setError(null);
    setPreview(null);
    setImportResult(null);
    setIsPreviewing(true);

    try {
      setPreview(await api.previewProductsImport(file));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Import preview failed");
    } finally {
      setIsPreviewing(false);
    }
  }

  function chooseFile(event: ChangeEvent<HTMLInputElement>) {
    const nextFile = event.target.files?.[0] ?? null;
    setFile(nextFile);
    setPreview(null);
    setImportResult(null);
    setError(null);
  }

  function resetFile() {
    setFile(null);
    setPreview(null);
    setImportResult(null);
    setError(null);
    if (inputRef.current) {
      inputRef.current.value = "";
    }
  }

  async function applyImport() {
    if (!file) {
      setError("Выберите Excel-файл");
      return;
    }

    if (!preview || preview.errorCount > 0) {
      setError("Сначала исправьте ошибки preview");
      return;
    }

    setError(null);
    setImportResult(null);
    setIsImporting(true);

    try {
      const result = await api.importProducts(file);
      setImportResult(result);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.productsRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.adminProductsRoot })
      ]);
      await onChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Import failed");
    } finally {
      setIsImporting(false);
    }
  }

  return (
    <ImportPanel>
      <PanelTop>
        <PanelTitle>
          <FileSpreadsheet size={18} />
          <strong>Импорт/экспорт</strong>
        </PanelTitle>
        <PanelActions>
          <GhostButton type="button" disabled={isExporting} onClick={() => void downloadExport()}>
            <Download size={16} /> {isExporting ? "Готовлю..." : "Скачать Excel"}
          </GhostButton>
          <FileButton>
            <Upload size={16} /> {file ? "Заменить файл" : "Выбрать Excel"}
            <input ref={inputRef} type="file" accept=".xlsx" onChange={chooseFile} />
          </FileButton>
          <PrimaryButton type="button" disabled={!file || isPreviewing} onClick={() => void previewImport()}>
            <CheckCircle2 size={16} /> {isPreviewing ? "Проверяю..." : "Проверить"}
          </PrimaryButton>
          <PrimaryButton type="button" disabled={!canApplyImport} onClick={() => void applyImport()}>
            <Upload size={16} /> {isImporting ? "Применяю..." : "Применить импорт"}
          </PrimaryButton>
        </PanelActions>
      </PanelTop>

      {file && (
        <SelectedFile>
          <span>{file.name}</span>
          <button type="button" onClick={resetFile}>Сбросить</button>
        </SelectedFile>
      )}

      {error && <FormError>{error}</FormError>}
      {importResult && (
        <ImportSuccess>
          Импорт применен: строк {importResult.totalRows}, создано {importResult.createdCount}, обновлено {importResult.updatedCount}, остатки {importResult.stockUpdatedCount}.
        </ImportSuccess>
      )}
      {preview && (
        <>
          <SummaryGrid>
            <SummaryItem>
              <span>Строк</span>
              <strong>{preview.totalRows}</strong>
            </SummaryItem>
            <SummaryItem>
              <span>Создать</span>
              <strong>{preview.createCount}</strong>
            </SummaryItem>
            <SummaryItem>
              <span>Обновить</span>
              <strong>{preview.updateCount}</strong>
            </SummaryItem>
            <SummaryItem $danger={preview.errorCount > 0}>
              <span>Ошибки</span>
              <strong>{preview.errorCount}</strong>
            </SummaryItem>
          </SummaryGrid>

          {preview.errors.length > 0 && (
            <GlobalErrors>
              {preview.errors.map((message) => (
                <li key={message}>
                  <AlertTriangle size={15} /> {message}
                </li>
              ))}
            </GlobalErrors>
          )}

          <PreviewTableWrap>
            <PreviewTable>
              <thead>
                <tr>
                  <th>Строка</th>
                  <th>SKU</th>
                  <th>Товар</th>
                  <th>Категория</th>
                  <th>Цена</th>
                  <th>Остаток</th>
                  <th>Действие</th>
                  <th>Ошибки</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((row) => (
                  <PreviewRow key={`${row.rowNumber}-${row.sku}`} row={row} />
                ))}
              </tbody>
            </PreviewTable>
          </PreviewTableWrap>
        </>
      )}
    </ImportPanel>
  );
}

function PreviewRow({ row }: { row: ProductImportPreviewRow }) {
  return (
    <tr>
      <td>{row.rowNumber}</td>
      <td>{row.sku || "-"}</td>
      <td>{row.name || "-"}</td>
      <td>{row.categoryCode || "-"}</td>
      <td>{row.price ?? "-"}</td>
      <td>{row.stockQuantity ?? "-"}</td>
      <td>
        <ActionBadge $action={row.action}>{actionLabel(row.action)}</ActionBadge>
      </td>
      <td>
        {row.errors.length === 0 ? (
          <OkText>OK</OkText>
        ) : (
          <ErrorList>
            {row.errors.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ErrorList>
        )}
      </td>
    </tr>
  );
}

function actionLabel(action: string) {
  if (action === "Create") return "Создать";
  if (action === "Update") return "Обновить";
  if (action === "Error") return "Ошибка";
  return action;
}

const ImportPanel = styled.section`
  display: grid;
  gap: 12px;
  border: 1px solid #d8e0de;
  border-radius: 8px;
  padding: 12px;
  background: #f8fbfa;
`;

const PanelTop = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;

  @media (max-width: 840px) {
    align-items: stretch;
    flex-direction: column;
  }
`;

const PanelTitle = styled.div`
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #172026;

  strong {
    font-size: 16px;
  }
`;

const PanelActions = styled.div`
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px;

  @media (max-width: 840px) {
    display: grid;
    grid-template-columns: 1fr;
  }
`;

const FileButton = styled.label`
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 40px;
  border-radius: 8px;
  padding: 9px 12px;
  color: #172026;
  background: #dfe8e5;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.16s ease;

  &:hover {
    background: #d0ddd9;
  }

  input {
    position: absolute;
    width: 1px;
    height: 1px;
    overflow: hidden;
    opacity: 0;
    pointer-events: none;
  }
`;

const SelectedFile = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  border-radius: 8px;
  padding: 8px 10px;
  color: #3f4d49;
  background: #ffffff;
  font-size: 14px;

  span {
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  button {
    border: 0;
    padding: 0;
    color: #238466;
    background: transparent;
    font-weight: 700;
  }
`;

const SummaryGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 8px;

  @media (max-width: 760px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
`;

const SummaryItem = styled.div<{ $danger?: boolean }>`
  display: grid;
  gap: 3px;
  border: 1px solid ${({ $danger }) => ($danger ? "#f0b8ae" : "#d8e0de")};
  border-radius: 8px;
  padding: 9px 10px;
  background: #ffffff;

  span {
    color: #64736e;
    font-size: 12px;
  }

  strong {
    color: ${({ $danger }) => ($danger ? "#9a342b" : "#172026")};
    font-size: 20px;
    line-height: 1;
  }
`;

const GlobalErrors = styled.ul`
  display: grid;
  gap: 6px;
  margin: 0;
  border: 1px solid #f0b8ae;
  border-radius: 8px;
  padding: 9px 10px;
  color: #9a342b;
  background: #fff1ef;
  list-style: none;
  font-size: 13px;

  li {
    display: flex;
    align-items: flex-start;
    gap: 6px;
  }
`;

const ImportSuccess = styled.div`
  border: 1px solid #a6dbc5;
  border-radius: 8px;
  padding: 9px 10px;
  color: #0f6048;
  background: #e5f6ef;
  font-size: 13px;
  font-weight: 700;
  line-height: 1.35;
`;

const PreviewTableWrap = styled.div`
  overflow-x: auto;
  border: 1px solid #d8e0de;
  border-radius: 8px;
  background: #ffffff;
`;

const PreviewTable = styled.table`
  width: 100%;
  min-width: 880px;
  border-collapse: collapse;
  font-size: 13px;

  th,
  td {
    padding: 9px 10px;
    border-bottom: 1px solid #edf1f0;
    text-align: left;
    vertical-align: top;
  }

  th {
    color: #64736e;
    background: #f3f6f5;
    font-weight: 800;
  }

  tbody tr:last-child td {
    border-bottom: 0;
  }
`;

const ActionBadge = styled(SoftBadge)<{ $action: string }>`
  color: ${({ $action }) => {
    if ($action === "Error") return "#9a342b";
    if ($action === "Create") return "#0f6048";
    return "#654a02";
  }};
  background: ${({ $action }) => {
    if ($action === "Error") return "#fff1ef";
    if ($action === "Create") return "#e5f6ef";
    return "#fff5df";
  }};
`;

const ErrorList = styled.ul`
  display: grid;
  gap: 3px;
  margin: 0;
  padding-left: 16px;
  color: #9a342b;
`;

const OkText = styled.span`
  color: #238466;
  font-weight: 700;
`;
