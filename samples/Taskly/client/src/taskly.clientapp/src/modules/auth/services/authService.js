import axios from 'axios';

const API_URL = 'https://example.com/api/auth';

const authService = {
  login: async (credentials) => {
    const { data } = await axios.post(`${API_URL}/login`, credentials);
    return data.user;
  },
  register: async (userDetails) => {
    const { data } = await axios.post(`${API_URL}/register`, userDetails);
    return data.user;
  },
};

export default authService;