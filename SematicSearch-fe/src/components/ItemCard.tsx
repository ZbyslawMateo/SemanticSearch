import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { MatrixColumn, SearchResponseItem } from "@/types";

type CardItem = SearchResponseItem | MatrixColumn;

function isPost(item: CardItem): item is SearchResponseItem {
  return "similarity" in item;
}

export function ItemCard({ item }: { item: CardItem }) {
  const post = isPost(item);

  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-2">
          <CardTitle className="text-sm font-medium leading-snug">
            {item.title}
          </CardTitle>

          {post ? (
            <Badge variant="secondary" className="shrink-0 text-xs font-mono">
              {(item.similarity * 100).toFixed(1)}%
            </Badge>
          ) : null}
        </div>
      </CardHeader>

      <CardContent className="text-xs text-muted-foreground space-y-2">
        {"description" in item && item.description ? (
          <p className="line-clamp-2">{item.description}</p>
        ) : null}

        {!post && (
          <div className="flex gap-2 pt-1 flex-wrap">
            {item.brand && <Badge variant="outline">{item.brand}</Badge>}
            {item.color && <Badge variant="outline">{item.color}</Badge>}
          </div>
        )}

        {post && item.tags.length > 0 && (
          <div className="flex gap-2 pt-1 flex-wrap">
            {item.tags.map((tag) => (
              <Badge key={tag} variant="outline">
                {tag}
              </Badge>
            ))}
          </div>
        )}

        {post && item.items.length > 0 && (
          <div className="pt-2 space-y-1">
            <p className="text-[11px] font-medium text-foreground">Items</p>
            <div className="flex flex-wrap gap-2">
              {item.items.map((subItem) => (
                <Badge
                  key={subItem.id}
                  variant="secondary"
                  className="max-w-full"
                >
                  {subItem.title}
                </Badge>
              ))}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
