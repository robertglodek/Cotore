import axios from 'axios';


// Create an Axios instance
const api = axios.create({
  baseURL: 'https://api.example.com',
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 5000
});

function getToken() {
  return localStorage.getItem('token');
}

// Centralized error handler with detailed return
function handleApiError(error) {
  if (error.response) {
    const { status, data } = error.response;
    
    if (status === 401) {
      // Token expired or unauthorized access
      window.location.href = '/login';
    } else if (status === 404) {
      // Not found error
      window.location.href = '/notFound';
    } else if (status === 500) {
      // Internal server error
      window.location.href = '/error';
    } 
    return Promise.reject({ status, data });
  } else {
    // Handle non-response errors (e.g., network issues)
    window.location.href = '/error';
    return Promise.reject(error); // Reject the error for downstream handling
  }
}

// Add a request interceptor to include Bearer token if it exists
api.interceptors.request.use(
  (config) => {
    const token = getToken();
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    return config;
  },
  (error) => Promise.reject(error)
);

// Response Interceptor for centralized handling and transformation
api.interceptors.response.use(
  (response) => {
    console.log('Response Data:', response.data);
    return { data: response.data, status: response.status }; // Optional wrapping
  },
  (error) => handleApiError(error)
);

// Generic GET request
export async function get(endpoint, params = {}) {
  const response = await api.get(endpoint, { params });
  return response.data;
}

// Generic POST request
export async function post(endpoint, data) {
  const response = await api.post(endpoint, data);
    return response.data;
}

// Generic PUT request
export async function put(endpoint, data) {
  const response = await api.put(endpoint, data);
    return response.data;
}

// Generic PATCH request
export async function patch(endpoint, data) {
  const response = await api.patch(endpoint, data);
  return response.data;
}
  
// Generic DELETE request
export async function del(endpoint) {
  const response = await api.delete(endpoint);
    return response.data;
}

export default api;