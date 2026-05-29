import request from '../../utils/request'

export interface FileRecord {
  id: number
  name: string
  originalName: string
  path: string
  url: string
  size: number
  mimeType: string
  extension: string
  folderId: number
  createTime: string
}

export function getFileList(folderId?: number): Promise<FileRecord[]> {
  return request.get('/file/list', { params: { folderId } })
}

export function uploadFile(formData: FormData, onProgress?: (percent: number) => void): Promise<FileRecord> {
  return request.post('/file/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
    onUploadProgress: onProgress
      ? (progressEvent) => {
          const percent = Math.round((progressEvent.loaded * 100) / (progressEvent.total || 1))
          onProgress(percent)
        }
      : undefined,
  })
}

export function downloadFile(id: number): Promise<void> {
  return request.get(`/file/download/${id}`, { responseType: 'blob' })
}

export function previewUrl(id: number): string {
  return `/api/file/preview/${id}`
}

export function deleteFile(id: number): Promise<void> {
  return request.delete(`/file/${id}`)
}

export function renameFile(id: number, name: string): Promise<void> {
  return request.put(`/file/${id}/rename`, { name })
}

export function moveFile(id: number, folderId: number): Promise<void> {
  return request.put(`/file/${id}/move`, { folderId })
}