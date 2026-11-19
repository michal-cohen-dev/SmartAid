import { createSlice } from '@reduxjs/toolkit';
import { initialState } from './requestsState';

import {
  clearOpenRequestsReducer,
  fetchOpenRequestsPendingReducer,
  fetchOpenRequestsFulfilledReducer,
  fetchOpenRequestsRejectedReducer,
} from './requestsReducers';

import { fetchOpenRequests } from './requestsThunks';

const requestsSlice = createSlice({
  name: 'requests',
  initialState,

  reducers: {
    clearOpenRequests: clearOpenRequestsReducer,
  },

  extraReducers: (builder) => {
    builder
      .addCase(
        fetchOpenRequests.pending,
        fetchOpenRequestsPendingReducer
      )
      .addCase(
        fetchOpenRequests.fulfilled,
        fetchOpenRequestsFulfilledReducer
      )
      .addCase(
        fetchOpenRequests.rejected,
        fetchOpenRequestsRejectedReducer
      );
  },
});

export const { clearOpenRequests } = requestsSlice.actions;

export default requestsSlice.reducer;