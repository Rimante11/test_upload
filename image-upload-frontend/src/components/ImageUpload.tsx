import React, { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import styled from 'styled-components';
import { imageApi, ImageUploadResponse } from '../api';

const Container = styled.div`
  max-width: 800px;
  margin: 2rem auto;
  padding: 2rem;
`;

const Title = styled.h2`
  color: #333;
  margin-bottom: 2rem;
`;

const DropzoneContainer = styled.div<{ $isDragActive: boolean }>`
  border: 2px dashed ${props => props.$isDragActive ? '#007bff' : '#ddd'};
  border-radius: 8px;
  padding: 3rem 2rem;
  text-align: center;
  cursor: pointer;
  background-color: ${props => props.$isDragActive ? '#f0f8ff' : '#fafafa'};
  transition: all 0.2s ease;
  
  &:hover {
    border-color: #007bff;
    background-color: #f0f8ff;
  }
`;

const DropzoneText = styled.p`
  margin: 0;
  color: #666;
  font-size: 1.1rem;
`;

const Form = styled.form`
  margin-top: 2rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
`;

const Input = styled.input`
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
`;

const TextArea = styled.textarea`
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  min-height: 100px;
  resize: vertical;
`;

const Button = styled.button`
  padding: 0.75rem 1.5rem;
  background-color: #28a745;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  align-self: flex-start;
  
  &:hover {
    background-color: #218838;
  }
  
  &:disabled {
    background-color: #ccc;
    cursor: not-allowed;
  }
`;

const PreviewContainer = styled.div`
  margin-top: 2rem;
  padding: 1rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: #f9f9f9;
`;

const PreviewImage = styled.img`
  max-width: 100%;
  max-height: 200px;
  border-radius: 4px;
`;

const FileInfo = styled.div`
  margin-top: 1rem;
  font-size: 0.9rem;
  color: #666;
`;

const ErrorMessage = styled.div`
  color: #dc3545;
  margin-top: 1rem;
`;

const SuccessMessage = styled.div`
  color: #28a745;
  margin-top: 1rem;
  padding: 1rem;
  background-color: #d4edda;
  border: 1px solid #c3e6cb;
  border-radius: 4px;
`;

interface ImageUploadProps {
  onUploadSuccess?: (image: ImageUploadResponse) => void;
}

const ImageUpload: React.FC<ImageUploadProps> = ({ onUploadSuccess }) => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [description, setDescription] = useState('');
  const [tags, setTags] = useState('');
  const [uploading, setUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState<ImageUploadResponse | null>(null);
  const [error, setError] = useState('');

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const file = acceptedFiles[0];
    if (file) {
      setSelectedFile(file);
      setPreviewUrl(URL.createObjectURL(file));
      setUploadResult(null);
      setError('');
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'image/*': ['.jpeg', '.jpg', '.png', '.gif', '.bmp', '.webp']
    },
    multiple: false,
    maxSize: 10 * 1024 * 1024, // 10MB
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedFile) return;

    setUploading(true);
    setError('');

    try {
      const result = await imageApi.uploadImage(selectedFile, description, tags);
      setUploadResult(result);
      onUploadSuccess?.(result);
      
      // Reset form
      setSelectedFile(null);
      setPreviewUrl(null);
      setDescription('');
      setTags('');
    } catch (error: any) {
      setError(error.response?.data?.message || 'Upload failed. Please try again.');
    } finally {
      setUploading(false);
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <Container>
      <Title>Upload New Image</Title>
      
      <DropzoneContainer {...getRootProps()} $isDragActive={isDragActive}>
        <input {...getInputProps()} />
        {isDragActive ? (
          <DropzoneText>Drop the image here...</DropzoneText>
        ) : (
          <DropzoneText>
            Drag & drop an image here, or click to select one
            <br />
            <small>Supported formats: JPEG, PNG, GIF, BMP, WebP (max 10MB)</small>
          </DropzoneText>
        )}
      </DropzoneContainer>

      {selectedFile && previewUrl && (
        <PreviewContainer>
          <PreviewImage src={previewUrl} alt="Preview" />
          <FileInfo>
            <strong>File:</strong> {selectedFile.name}<br />
            <strong>Size:</strong> {formatFileSize(selectedFile.size)}<br />
            <strong>Type:</strong> {selectedFile.type}
          </FileInfo>
          
          <Form onSubmit={handleSubmit}>
            <Input
              type="text"
              placeholder="Description (optional)"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
            <TextArea
              placeholder="Tags (comma-separated, optional)"
              value={tags}
              onChange={(e) => setTags(e.target.value)}
            />
            <Button type="submit" disabled={uploading}>
              {uploading ? 'Uploading...' : 'Upload Image'}
            </Button>
          </Form>
        </PreviewContainer>
      )}

      {error && <ErrorMessage>{error}</ErrorMessage>}

      {uploadResult && (
        <SuccessMessage>
          <strong>Image uploaded successfully!</strong>
          <div>File: {uploadResult.originalFileName}</div>
          <div>Size: {uploadResult.width} × {uploadResult.height}px</div>
          <div>Uploaded: {new Date(uploadResult.uploadedAt).toLocaleString()}</div>
        </SuccessMessage>
      )}
    </Container>
  );
};

export default ImageUpload;