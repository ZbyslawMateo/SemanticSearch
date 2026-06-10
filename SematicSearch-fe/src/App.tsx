import { useState } from "react";
import { SearchBar } from "@/components/SearchBar";
import { SearchResults } from "@/components/SearchResults";
import { search } from "@/api/search";
import type { SearchResponse } from "@/types";

export default function App() {
  const [data, setData] = useState<SearchResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSearch = async (query: string) => {
    setLoading(true);
    setError(null);
    try {
      const result = await search(query);
      setData(result);
    } catch {
      setError("Search failed — is the backend running?");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-background text-foreground">
      <div className="max-w-6xl mx-auto px-4 py-12">
        <div className="text-center mb-10">
          <h1 className="text-3xl font-bold tracking-tight mb-2">
            Semantic Search
          </h1>
          <p className="text-muted-foreground text-sm">
            Search items and posts by meaning, not keywords
          </p>
        </div>

        <SearchBar onSearch={handleSearch} loading={loading} />

        {error && (
          <p className="text-center text-sm text-destructive mt-4">{error}</p>
        )}

        <SearchResults data={data} loading={loading} />
      </div>
    </div>
  );
}
