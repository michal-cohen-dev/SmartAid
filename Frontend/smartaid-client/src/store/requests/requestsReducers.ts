
import type { PayloadAction } from '@reduxjs/toolkit';
import type { HelpRequest, RequestsState } from './requestsState';
export const clearOpenRequestsReducer = (
  state: RequestsState
) => {
  state.openRequests = [];
};

export const fetchOpenRequestsPendingReducer = (
  state: RequestsState
) => {
  state.loading = true;
  state.error = null;
};

export const fetchOpenRequestsFulfilledReducer = (
  state: RequestsState,
  action: PayloadAction<HelpRequest[]>
) => {
  state.loading = false;
  state.openRequests = action.payload;
};

export const fetchOpenRequestsRejectedReducer = (
  state: RequestsState,
  action: PayloadAction<string | undefined>
) => {
  state.loading = false;
  state.error = action.payload ?? 'שגיאה בטעינת בקשות העזרה';
};
