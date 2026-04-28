export interface SuggestionShrine {
  name: string;
}

export interface SearchShrinesParams {
  shrine?: string;
  location?: string;
  latitude?: number;
  longitude?: number;
  cursor?: string;
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
