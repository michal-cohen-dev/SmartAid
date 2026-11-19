
import { createAsyncThunk } from '@reduxjs/toolkit';
import { apiRequest } from '../../services/api';
import type { HelpRequest } from './requestsState';

export const fetchOpenRequests = createAsyncThunk<
  HelpRequest[],
  void,
  { rejectValue: string }
>(
  'requests/fetchOpenRequests',
  async (_, { rejectWithValue }) => {
    try {
      const data = await apiRequest<HelpRequest[]>(
        '/api/HelpRequests/open'
      );

      const validRequests = data.filter(
        request =>
          request.title?.trim() &&
          request.description?.trim()
      );

      return validRequests;
    } catch (error) {
      return rejectWithValue(
        error instanceof Error
          ? error.message
          : 'אירעה שגיאה בטעינת בקשות העזרה'
      );
    }
  }
);
