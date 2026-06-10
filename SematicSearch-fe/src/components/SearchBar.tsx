import { useState, type SetStateAction } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Search } from "lucide-react";

interface Props {
  onSearch: (q: string) => void;
  loading: boolean;
}

export function SearchBar({ onSearch, loading }: Props) {
  const [value, setValue] = useState("");

  const handle = () => {
    if (value.trim()) onSearch(value.trim());
  };

  return (
    <div className="flex gap-2 w-full max-w-2xl mx-auto">
      <Input
        placeholder="Search items, posts, brands..."
        value={value}
        onChange={(e: { target: { value: SetStateAction<string>; }; }) => setValue(e.target.value)}
        onKeyDown={(e: { key: string; }) => e.key === "Enter" && handle()}
        className="text-base"
      />
      <Button onClick={handle} disabled={loading}>
        <Search className="w-4 h-4 mr-2" />
        {loading ? "Searching..." : "Search"}
      </Button>
    </div>
  );
}
