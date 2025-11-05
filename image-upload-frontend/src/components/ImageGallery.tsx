import React, { useState, useEffect } from 'react';
import styled from 'styled-components';
import { imageApi, ImageListResponse, ImageUploadResponse } from '../api';

const Container = styled.div`
  max-width: 1200px;
  margin: 2rem auto;
  padding: 2rem;
`;

const Title = styled.h2`
  color: #333;
  margin-bottom: 2rem;
`;

const GridContainer = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1.5rem;
`;

const ImageCard = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.2s ease;
  cursor: pointer;
  
  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  }
`;

const ImageContainer = styled.div`
  width: 100%;
  height: 200px;
  overflow: hidden;
  background-color: #f5f5f5;
`;

const Image = styled.img`
  width: 100%;
  height: 100%;
  object-fit: cover;
`;

const ImageInfo = styled.div`
  padding: 1rem;
`;

const ImageTitle = styled.h3`
  margin: 0 0 0.5rem 0;
  font-size: 1rem;
  color: #333;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
`;

const ImageDescription = styled.p`
  margin: 0 0 0.5rem 0;
  font-size: 0.9rem;
  color: #666;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
`;

const ImageDate = styled.div`
  font-size: 0.8rem;
  color: #999;
`;

const LoadingContainer = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
  font-size: 1.1rem;
  color: #666;
`;

const ErrorContainer = styled.div`
  background-color: #f8d7da;
  color: #721c24;
  padding: 1rem;
  border-radius: 4px;
  text-align: center;
`;

const EmptyContainer = styled.div`
  text-align: center;
  padding: 3rem;
  color: #666;
`;

const Modal = styled.div<{ isOpen: boolean }>`
  display: ${props => props.isOpen ? 'flex' : 'none'};
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.8);
  z-index: 1000;
  align-items: center;
  justify-content: center;
  padding: 2rem;
`;

const ModalContent = styled.div`
  background: white;
  border-radius: 8px;
  max-width: 90%;
  max-height: 90%;
  overflow: auto;
  position: relative;
`;

const CloseButton = styled.button`
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: rgba(0, 0, 0, 0.5);
  color: white;
  border: none;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  cursor: pointer;
  font-size: 1.2rem;
  z-index: 1001;
  
  &:hover {
    background: rgba(0, 0, 0, 0.7);
  }
`;

const FullImage = styled.img`
  max-width: 100%;
  height: auto;
  display: block;
`;

const ModalInfo = styled.div`
  padding: 1rem;
`;

const DeleteButton = styled.button`
  background-color: #dc3545;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  margin-top: 1rem;
  
  &:hover {
    background-color: #c82333;
  }
`;

interface ImageGalleryProps {
  refreshTrigger?: number;
}

const ImageGallery: React.FC<ImageGalleryProps> = ({ refreshTrigger }) => {
  const [images, setImages] = useState<ImageListResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedImage, setSelectedImage] = useState<ImageUploadResponse | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  const loadImages = async () => {
    try {
      setLoading(true);
      const imageList = await imageApi.getImages();
      setImages(imageList);
      setError('');
    } catch (error: any) {
      setError('Failed to load images. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadImages();
  }, [refreshTrigger]);

  const handleImageClick = async (imageId: number) => {
    try {
      const imageDetail = await imageApi.getImageById(imageId);
      setSelectedImage(imageDetail);
      setModalOpen(true);
    } catch (error) {
      console.error('Error loading image details:', error);
    }
  };

  const handleDeleteImage = async (imageId: number) => {
    if (window.confirm('Are you sure you want to delete this image?')) {
      try {
        await imageApi.deleteImage(imageId);
        setModalOpen(false);
        setSelectedImage(null);
        loadImages(); // Refresh the list
      } catch (error) {
        alert('Failed to delete image. Please try again.');
      }
    }
  };

  const closeModal = () => {
    setModalOpen(false);
    setSelectedImage(null);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (loading) {
    return (
      <Container>
        <LoadingContainer>Loading images...</LoadingContainer>
      </Container>
    );
  }

  if (error) {
    return (
      <Container>
        <ErrorContainer>{error}</ErrorContainer>
      </Container>
    );
  }

  return (
    <Container>
      <Title>My Images ({images.length})</Title>
      
      {images.length === 0 ? (
        <EmptyContainer>
          <p>No images uploaded yet.</p>
          <p>Upload your first image to get started!</p>
        </EmptyContainer>
      ) : (
        <GridContainer>
          {images.map((image) => (
            <ImageCard
              key={image.id}
              onClick={() => handleImageClick(image.id)}
            >
              <ImageContainer>
                <Image
                  src={image.thumbnailUrl}
                  alt={image.originalFileName}
                  loading="lazy"
                />
              </ImageContainer>
              <ImageInfo>
                <ImageTitle>{image.originalFileName}</ImageTitle>
                {image.description && (
                  <ImageDescription>{image.description}</ImageDescription>
                )}
                <ImageDate>{formatDate(image.uploadedAt)}</ImageDate>
              </ImageInfo>
            </ImageCard>
          ))}
        </GridContainer>
      )}

      <Modal isOpen={modalOpen} onClick={closeModal}>
        <ModalContent onClick={(e) => e.stopPropagation()}>
          <CloseButton onClick={closeModal}>×</CloseButton>
          {selectedImage && (
            <>
              <FullImage
                src={selectedImage.originalImageUrl}
                alt={selectedImage.originalFileName}
              />
              <ModalInfo>
                <h3>{selectedImage.originalFileName}</h3>
                {selectedImage.description && (
                  <p><strong>Description:</strong> {selectedImage.description}</p>
                )}
                {selectedImage.tags && (
                  <p><strong>Tags:</strong> {selectedImage.tags}</p>
                )}
                <p><strong>Size:</strong> {selectedImage.width} × {selectedImage.height}px</p>
                <p><strong>File size:</strong> {Math.round(selectedImage.fileSizeBytes / 1024)} KB</p>
                <p><strong>Uploaded:</strong> {formatDate(selectedImage.uploadedAt)}</p>
                <DeleteButton onClick={() => handleDeleteImage(selectedImage.id)}>
                  Delete Image
                </DeleteButton>
              </ModalInfo>
            </>
          )}
        </ModalContent>
      </Modal>
    </Container>
  );
};

export default ImageGallery;