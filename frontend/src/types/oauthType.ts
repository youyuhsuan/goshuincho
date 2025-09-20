export interface StateData {
  uuid: string
  timestamp: number
  return_url: string
}

export interface PKCEData {
  codeVerifier: string
  state: string
  timestamp: number
  expiresAt: number
  inProgress: boolean
  flowId: string
}
