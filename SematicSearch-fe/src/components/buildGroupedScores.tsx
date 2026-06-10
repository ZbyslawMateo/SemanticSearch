import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { SimilarityMatrix } from "./SimilarityMatrix";
import { ItemCard } from "./ItemCard";
import { GroupedResultsTable } from "./GroupedResultsTable";
import type {
  GroupedMatrixColumn,
  SearchResponse,
  SearchResponseItem,
} from "@/types";

interface Props {
  data: SearchResponse | null;
  loading: boolean;
}

function buildGroupedColumns(
  responses: SearchResponseItem[]
): GroupedMatrixColumn[] {
  const postColumns: GroupedMatrixColumn[] = responses.map((x) => ({
    key: `post-${x.id}`,
    label: x.title,
    group: "Post",
  }));

  const tagColumns: GroupedMatrixColumn[] = Array.from(
    new Set(responses.flatMap((x) => x.tags))
  ).map((tag) => ({
    key: `tag-${tag}`,
    label: tag,
    group: "Tags",
  }));

  const itemColumns: GroupedMatrixColumn[] = responses
    .flatMap((x) => x.items)
    .filter(
      (item, index, arr) => arr.findIndex((i) => i.id === item.id) === index
    )
    .map((item) => ({
      key: `item-${item.id}`,
      label: [item.title, item.brand, item.color].filter(Boolean).join(" · "),
      group: "Items",
    }));

  return [...postColumns, ...tagColumns, ...itemColumns];
}

function buildGroupedScores(
  responses: SearchResponseItem[],
  columns: GroupedMatrixColumn[]
): number[][] {
  return responses.map((response) =>
    columns.map((column) => {
      if (column.group === "Post") {
        return column.key === `post-${response.id}` ? response.similarity : 0;
      }

      if (column.group === "Tags") {
        const tag = column.key.replace("tag-", "");
        return response.tags.includes(tag) ? response.similarity : 0;
      }

      if (column.group === "Items") {
        const itemId = column.key.replace("item-", "");
        return response.items.some((item) => item.id === itemId)
          ? response.similarity
          : 0;
      }

      return 0;
    })
  );
}

export function SearchResults({ data, loading }: Props) {
  if (loading) {
    return (
      <div className="space-y-3 mt-6">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-24 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!data) return null;

  const responses = data.results ?? [];
  const backendItems = data.matrix?.columns ?? [];

  const groupedColumns = buildGroupedColumns(responses);
  const groupedScores = buildGroupedScores(responses, groupedColumns);

  return (
    <div className="mt-6 space-y-6">
      <Tabs defaultValue="grouped">
        <TabsList>
          <TabsTrigger value="grouped">
            Grouped Results ({responses.length})
          </TabsTrigger>
          <TabsTrigger value="responses">Cards</TabsTrigger>
          <TabsTrigger value="items">Items ({backendItems.length})</TabsTrigger>
          <TabsTrigger value="matrix">Similarity Matrix</TabsTrigger>
        </TabsList>

        <TabsContent value="grouped" className="mt-4">
          <GroupedResultsTable results={responses} query={data.query} />
        </TabsContent>

        <TabsContent
          value="responses"
          className="mt-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3"
        >
          {responses.map((response) => (
            <ItemCard key={response.id} item={response} />
          ))}
        </TabsContent>

        <TabsContent
          value="items"
          className="mt-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3"
        >
          {backendItems.map((item) => (
            <ItemCard key={item.id} item={item} />
          ))}
        </TabsContent>

        <TabsContent value="matrix" className="mt-4">
          <p className="text-xs text-muted-foreground mb-3">
            Y axis = Responses · X axis = grouped post, tag, and item features
          </p>
          <SimilarityMatrix
            rowLabels={responses.map((x) => x.title)}
            columns={groupedColumns}
            scores={groupedScores}
          />
        </TabsContent>
      </Tabs>
    </div>
  );
}
