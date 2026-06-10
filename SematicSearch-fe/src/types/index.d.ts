export interface SearchItem {
    id: string
    title: string
    description: string
    brand: string | null
    color: string | null
}

export interface SearchResponseItem {
    id: string
    title: string
    description: string
    similarity: number
    tags: string[]
    items: SearchItem[]
}

export interface BackendMatrixRow {
    id: string
    title: string
    similarity: number
}

export interface BackendMatrixColumn {
    id: string
    title: string
    brand: string | null
    color: string | null
}

export interface RankingEntry {
    type: "post" | "item" | "tag"
    label: string
    detail: string | null
    similarity: number
}

export interface SearchResponse {
    query: string
    results: SearchResponseItem[]
    matrix: {
        rows: BackendMatrixRow[]
        columns: BackendMatrixColumn[]
        values: number[][]
    }
    rankings: RankingEntry[]
}

export interface GroupedMatrixColumn {
    key: string
    label: string
}