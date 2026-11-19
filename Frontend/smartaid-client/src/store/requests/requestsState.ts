
export interface HelpRequest {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  requiredEquipmentTypeIds?: number[];
  requiredEquipmentNames?: string[];
  status?: number;
  address?: string;
  assignedVolunteerName?: string | null;
}

export interface RequestsState {
  openRequests: HelpRequest[];
  loading: boolean;
  error: string | null;
}

export const initialState: RequestsState = {
  openRequests: [],
  loading: false,
  error: null,
};
