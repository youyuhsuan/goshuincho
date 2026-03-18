interface BaseAuthCredentials {
  email: string;
  password: string;
}

export interface LoginRequest extends BaseAuthCredentials {
  rememberMe?: boolean;
}

export interface RegisterFormData extends BaseAuthCredentials {
  name: string;
  confirmPassword: string;
}

export interface RegisterRequest extends BaseAuthCredentials {
  name: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}
