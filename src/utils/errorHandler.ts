import { AxiosError, type AxiosResponse } from 'axios'

/**
 * Formats validation errors from API responses into user-readable format
 * @param errors - Validation errors from API response (various formats)
 * @returns Formatted validation error string or null if no valid errors
 */
const formatValidationErrors = (errors: unknown): string | null => {
  if (typeof errors === 'object' && errors && !Array.isArray(errors)) {
    return Object.values(errors as Record<string, unknown>)
      .flat()
      .join(', ')
  }

  if (typeof errors === 'string') return errors
  return null
}

/**
 * Handles Axios-specific API errors including network and HTTP response errors
 * @param error - AxiosError instance containing request/response details
 * @returns Appropriate error message based on error code or HTTP status
 */
const handleApiError = (error: AxiosError<AxiosResponse>): string => {
  const { status, data } = error.response || {}

  // Handle network-level errors first
  if (error.code === AxiosError.ERR_NETWORK) return handleNetworkError()
  if (error.code === AxiosError.ERR_CANCELED) return 'Request was cancelled'
  if (error.code === AxiosError.ETIMEDOUT) return 'Request timed out'

  // Handle HTTP status code errors
  switch (status) {
    case 400:
      return 'Bad request parameters'
    case 401:
      return 'Please log in again'
    case 403:
      return 'Access denied - insufficient permissions'
    case 404:
      return 'Requested resource not found'
    case 422:
      return formatValidationErrors(data?.errors) || 'Validation failed'
    case 500:
      return 'Internal server error, please try again later'
    default:
      return data?.message || `Request failed with status ${status}`
  }
}

/**
 * Handles generic network connectivity issues
 * @returns Network error message with troubleshooting hint
 */
const handleNetworkError = (): string => {
  return 'Network connection failed, please check your internet connection'
}

/**
 * Handles errors that don't fit other categories
 * @param error - Unknown error object
 * @returns Generic error message, using error.message if available
 */
const handleUnknownError = (error: unknown): string => {
  if (error instanceof Error) return error.message || 'An unknown error occurred'
  return 'An unknown error occurred'
}

/**
 * Main error handling entry point
 * Determines error type and delegates to appropriate handler
 * @param error - The error to be handled (unknown type for maximum compatibility)
 * @returns User-friendly error message string
 */
const handleError = (error: unknown): string => {
  console.error('API Error:', error)

  if (error instanceof AxiosError) return handleApiError(error)
  if (error && typeof error === 'object' && 'code' in error && error.code === 'NETWORK_ERROR')
    return handleNetworkError()

  return handleUnknownError(error)
}

export default handleError
