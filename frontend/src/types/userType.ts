export interface User {
  id: string;
  name: string;
  email: string;
  picture?: string;
}

export interface UpdateRequest {
  name: string;
}
