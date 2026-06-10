import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import type { SearchResponseItem } from "@/types";

interface Props {
  results: SearchResponseItem[];
  query: string;
}

function scoreTone(score: number) {
  if (score >= 0.7) return "bg-red-600 text-white";
  if (score >= 0.5) return "bg-orange-500 text-white";
  if (score >= 0.35) return "bg-yellow-400 text-black";
  return "bg-slate-200 text-slate-700";
}

export function GroupedResultsTable({ results, query }: Props) {
  return (
    <div className="space-y-3">
      <div className="rounded-lg border bg-muted/30 px-4 py-3">
        <p className="text-xs text-muted-foreground">Query</p>
        <p className="text-sm font-medium">{query}</p>
      </div>

      <div className="space-y-3">
        {results.map((result, index) => (
          <Card key={result.id} className="p-4">
            <div className="flex flex-col gap-4 lg:flex-row lg:items-start">
              <div className="flex items-center gap-3 lg:w-40 lg:shrink-0">
                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-xs font-semibold">
                  {index + 1}
                </div>
                <Badge className={scoreTone(result.similarity)}>
                  {(result.similarity * 100).toFixed(1)}%
                </Badge>
              </div>

              <div className="min-w-0 flex-1 space-y-3">
                <div>
                  <h3 className="text-sm font-semibold leading-snug">
                    {result.title}
                  </h3>
                  {result.description ? (
                    <p className="mt-1 text-xs text-muted-foreground">
                      {result.description}
                    </p>
                  ) : null}
                </div>

                {result.tags.length > 0 && (
                  <div className="space-y-1">
                    <p className="text-[11px] font-medium text-muted-foreground uppercase tracking-wide">
                      Tags
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {result.tags.map((tag) => (
                        <Badge key={tag} variant="outline">
                          {tag}
                        </Badge>
                      ))}
                    </div>
                  </div>
                )}

                {result.items.length > 0 && (
                  <div className="space-y-1">
                    <p className="text-[11px] font-medium text-muted-foreground uppercase tracking-wide">
                      Items
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {result.items.map((item) => {
                        const meta = [item.brand, item.color]
                          .filter(Boolean)
                          .join(" · ");
                        const label = meta
                          ? `${item.title} · ${meta}`
                          : item.title;

                        return (
                          <Badge
                            key={item.id}
                            variant="secondary"
                            className="max-w-full"
                          >
                            <span className="truncate">{label}</span>
                          </Badge>
                        );
                      })}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
