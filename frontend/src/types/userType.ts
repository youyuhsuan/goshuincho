export interface Me {
  id: string;
  name: string;
  email: string;
  picture?: string;
}

export interface User extends Omit<Me, "id"> {
  bio?: string;
  birthDate?: Date | null;
  location?: string;
  favoriteGoods?: string[];
}

export type UpdateRequest = Partial<User>;
