export interface SuggestionShrine {
  name: string;
}

export interface SearchShrinesParams {
  shrine?: string;
  latitude?: number;
  longitude?: number;
  page?: number;
  pageSize?: number;
}

export interface Shrine {
  id: string;
  name: string;
  imageUrl?: string;
  prefecture?: string;
  region?: string;
  city?: string;
}
