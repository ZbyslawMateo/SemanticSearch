import type { RankingEntry } from "@/types";

const TYPE_COLOR: Record<RankingEntry["type"], string> = {
  post: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300",
  item: "bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300",
  tag: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300",
};

function ScoreBar({ value }: { value: number }) {
  return (
    <div className="flex items-center gap-2 min-w-30">
      <div className="flex-1 h-2 rounded-full bg-muted overflow-hidden">
        <div
          className="h-full rounded-full bg-primary transition-all"
          style={{ width: `${(value * 100).toFixed(1)}%` }}
        />
      </div>
      <span className="text-xs font-mono text-muted-foreground w-10 text-right">
        {(value * 100).toFixed(1)}%
      </span>
    </div>
  );
}

export function RankTable({
  rankings,
  query,
}: {
  rankings: RankingEntry[];
  query: string;
}) {
  return (
    <div className="rounded-lg border bg-card overflow-hidden">
      <div className="px-4 py-3 border-b bg-muted/40 flex items-center gap-2">
        <span className="text-xs text-muted-foreground">Query:</span>
        <span className="text-xs font-medium">{query}</span>
      </div>

      <div className="divide-y">
        {rankings.map((entry, i) => (
          <div
            key={i}
            className="flex items-center gap-3 px-4 py-2.5 hover:bg-muted/30 transition-colors"
          >
            {/* Rank number */}
            <span className="text-xs font-mono text-muted-foreground w-5 shrink-0">
              {i + 1}
            </span>

            {/* Type badge */}
            <span
              className={`text-[10px] font-semibold uppercase tracking-wide px-1.5 py-0.5 rounded shrink-0 ${
                TYPE_COLOR[entry.type]
              }`}
            >
              {entry.type}
            </span>

            {/* Label + detail */}
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium truncate">{entry.label}</p>
              {entry.detail && (
                <p className="text-[10px] text-muted-foreground truncate">
                  {entry.detail}
                </p>
              )}
            </div>

            {/* Score bar */}
            <ScoreBar value={entry.similarity} />
          </div>
        ))}
      </div>
    </div>
  );
}
