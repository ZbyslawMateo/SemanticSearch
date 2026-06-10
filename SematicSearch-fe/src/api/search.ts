import axios from "axios";
import type { SearchResponse } from "@/types";

export async function search(query: string): Promise<SearchResponse> {
    const { data } = await axios.get<SearchResponse>("/api/posts/search", {
        params: { query },
    });

    return data;
}