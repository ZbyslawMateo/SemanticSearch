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
  return responses.map((response) => {
    const tagPart = response.tags.join(" · ");

    const itemPart = response.items
      .map((item) =>
        [item.title, item.brand, item.color].filter(Boolean).join(" · ")
      )
      .join(" | ");

    const parts = [response.title, tagPart, itemPart].filter(Boolean);

    return {
      key: `group-${response.id}`,
      label: parts.join(" | "),
    };
  });
}

function buildMatrixScores(
  responses: SearchResponseItem[],
  columns: GroupedMatrixColumn[]
): number[][] {
  return responses.map((response) =>
    columns.map((column) =>
      column.key === `group-${response.id}` ? response.similarity : 0
    )
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
  const matrixScores = buildMatrixScores(responses, groupedColumns);

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
            Y axis = Responses · X axis = grouped results with tags and item
            metadata
          </p>
          <SimilarityMatrix
            rowLabels={responses.map((x) => x.title)}
            columns={groupedColumns}
            scores={matrixScores}
          />
        </TabsContent>
      </Tabs>
    </div>
  );
}
