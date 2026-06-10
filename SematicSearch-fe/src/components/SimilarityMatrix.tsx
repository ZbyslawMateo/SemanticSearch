import type { GroupedMatrixColumn } from "@/types";

interface Props {
  columns: GroupedMatrixColumn[];
  scores: number[][];
  rowLabels: string[];
}

function interpolateColor(value: number): string {
  const stops = [
    [0.0, [239, 246, 255]],
    [0.3, [147, 197, 253]],
    [0.5, [250, 204, 21]],
    [0.7, [249, 115, 22]],
    [1.0, [185, 28, 28]],
  ] as [number, number[]][];

  let lower = stops[0];
  let upper = stops[stops.length - 1];

  for (let i = 0; i < stops.length - 1; i++) {
    if (value >= stops[i][0] && value <= stops[i + 1][0]) {
      lower = stops[i];
      upper = stops[i + 1];
      break;
    }
  }

  const range = upper[0] - lower[0] || 1;
  const t = (value - lower[0]) / range;
  const r = Math.round(lower[1][0] + t * (upper[1][0] - lower[1][0]));
  const g = Math.round(lower[1][1] + t * (upper[1][1] - lower[1][1]));
  const b = Math.round(lower[1][2] + t * (upper[1][2] - lower[1][2]));
  return `rgb(${r},${g},${b})`;
}

const CELL = 52;
const LABEL_W = 180;
const LABEL_H = 120;

export function SimilarityMatrix({ columns, scores, rowLabels }: Props) {
  const rows = scores.length;
  const cols = columns.length;

  const width = LABEL_W + cols * CELL + 20;
  const height = LABEL_H + rows * CELL + 24;

  return (
    <div className="overflow-auto rounded-lg border bg-card p-3">
      <svg width={width} height={height} className="font-sans text-xs">
        {columns.map((col, ci) => (
          <g
            key={col.key}
            transform={`translate(${LABEL_W + ci * CELL + CELL / 2}, ${
              LABEL_H - 8
            })`}
          >
            <text
              transform="rotate(-45)"
              textAnchor="start"
              className="fill-muted-foreground"
              fontSize={11}
            >
              {col.label.length > 34 ? `${col.label.slice(0, 34)}…` : col.label}
            </text>
          </g>
        ))}

        {rowLabels.map((lbl, ri) => (
          <text
            key={ri}
            x={LABEL_W - 10}
            y={LABEL_H + ri * CELL + CELL / 2 + 4}
            textAnchor="end"
            className="fill-muted-foreground"
            fontSize={11}
          >
            {lbl.length > 22 ? `${lbl.slice(0, 22)}…` : lbl}
          </text>
        ))}

        {scores.map((row, ri) =>
          row.map((val, ci) => {
            const x = LABEL_W + ci * CELL;
            const y = LABEL_H + ri * CELL;

            return (
              <g key={`${ri}-${ci}`}>
                <rect
                  x={x}
                  y={y}
                  width={CELL - 2}
                  height={CELL - 2}
                  rx={4}
                  fill={interpolateColor(val)}
                />
                <text
                  x={x + CELL / 2}
                  y={y + CELL / 2 + 4}
                  textAnchor="middle"
                  fontSize={10}
                  fill={val > 0.55 ? "#fff" : "#334155"}
                  fontWeight="600"
                >
                  {(val * 100).toFixed(0)}
                </text>
                <title>
                  {`${rowLabels[ri]} × ${columns[ci].label}: ${(
                    val * 100
                  ).toFixed(1)}%`}
                </title>
              </g>
            );
          })
        )}

        <text
          x={LABEL_W + (cols * CELL) / 2}
          y={height - 4}
          textAnchor="middle"
          fontSize={11}
          className="fill-muted-foreground"
        >
          Grouped results
        </text>

        <text
          transform={`translate(14, ${
            LABEL_H + (rows * CELL) / 2
          }) rotate(-90)`}
          textAnchor="middle"
          fontSize={11}
          className="fill-muted-foreground"
        >
          Responses
        </text>
      </svg>

      <div className="flex items-center gap-2 mt-3 px-2">
        <span className="text-xs text-muted-foreground">Low</span>
        <div
          className="h-3 w-40 rounded"
          style={{
            background:
              "linear-gradient(to right, rgb(239,246,255), rgb(250,204,21), rgb(185,28,28))",
          }}
        />
        <span className="text-xs text-muted-foreground">High</span>
      </div>
    </div>
  );
}
