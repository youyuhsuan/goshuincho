export interface User {
  id: string;
  name: string;
  email: string;
  picture?: string;
}

export interface UpdateRequst {
  name: string;
}
