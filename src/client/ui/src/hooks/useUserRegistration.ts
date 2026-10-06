import { useSurveySession } from "../api/SurveySession";

export function useUserRegistration() {
	return useSurveySession().registration;
}
