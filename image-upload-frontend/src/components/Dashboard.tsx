import React, { useState } from 'react';
import styled from 'styled-components';
import { useAuth } from '../contexts/AuthContext';
import ImageUpload from './ImageUpload';
import ImageGallery from './ImageGallery';
import { ImageUploadResponse } from '../api';

const Container = styled.div`
  min-height: 100vh;
  background-color: #f8f9fa;
`;

const Header = styled.header`
  background-color: #fff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  padding: 1rem 0;
`;

const HeaderContent = styled.div`
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 2rem;
`;

const Logo = styled.h1`
  color: #333;
  margin: 0;
  font-size: 1.5rem;
`;

const UserInfo = styled.div`
  display: flex;
  align-items: center;
  gap: 1rem;
`;

const UserDetails = styled.div`
  text-align: right;
`;

const Username = styled.div`
  font-weight: 600;
  color: #333;
`;

const TenantName = styled.div`
  font-size: 0.9rem;
  color: #666;
`;

const LogoutButton = styled.button`
  background-color: #dc3545;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  
  &:hover {
    background-color: #c82333;
  }
`;

const TabContainer = styled.div`
  max-width: 1200px;
  margin: 2rem auto;
  padding: 0 2rem;
`;

const TabButtons = styled.div`
  display: flex;
  gap: 0.5rem;
  margin-bottom: 2rem;
`;

const TabButton = styled.button<{ $active: boolean }>`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 6px 6px 0 0;
  background-color: ${props => props.$active ? '#007bff' : '#f8f9fa'};
  color: ${props => props.$active ? 'white' : '#666'};
  cursor: pointer;
  font-size: 1rem;
  border-bottom: 3px solid ${props => props.$active ? '#007bff' : 'transparent'};
  
  &:hover {
    background-color: ${props => props.$active ? '#0056b3' : '#e9ecef'};
  }
`;

const TabContent = styled.div`
  background-color: white;
  border-radius: 0 8px 8px 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  min-height: 400px;
`;

type Tab = 'upload' | 'gallery';

const Dashboard: React.FC = () => {
  const { user, logout } = useAuth();
  const [activeTab, setActiveTab] = useState<Tab>('upload');
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const handleUploadSuccess = (image: ImageUploadResponse) => {
    // Switch to gallery tab and refresh it
    setActiveTab('gallery');
    setRefreshTrigger(prev => prev + 1);
  };

  if (!user) {
    return null;
  }

  return (
    <Container>
      <Header>
        <HeaderContent>
          <Logo>Image Upload Platform</Logo>
          <UserInfo>
            <UserDetails>
              <Username>{user.username}</Username>
              <TenantName>{user.tenantName}</TenantName>
            </UserDetails>
            <LogoutButton onClick={logout}>
              Logout
            </LogoutButton>
          </UserInfo>
        </HeaderContent>
      </Header>

      <TabContainer>
        <TabButtons>
          <TabButton
            $active={activeTab === 'upload'}
            onClick={() => setActiveTab('upload')}
          >
            Upload Image
          </TabButton>
          <TabButton
            $active={activeTab === 'gallery'}
            onClick={() => setActiveTab('gallery')}
          >
            My Images
          </TabButton>
        </TabButtons>

        <TabContent>
          {activeTab === 'upload' && (
            <ImageUpload onUploadSuccess={handleUploadSuccess} />
          )}
          {activeTab === 'gallery' && (
            <ImageGallery refreshTrigger={refreshTrigger} />
          )}
        </TabContent>
      </TabContainer>
    </Container>
  );
};

export default Dashboard;