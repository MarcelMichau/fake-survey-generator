export type ExceptionMessage = {
    type: string;
    title: string;
    status: number;
    detail: string;
    traceId: string;
    errors: Record<string, string[]>;
};

export type ResponseException = {
    exceptionMessage: ExceptionMessage;
};

export type ApiVersionModel = {
    assemblyVersion: string;
    assemblyFileVersion: string;
    assemblyInformationalVersion: string;
    assemblyName: string;
    assemblyTitle: string;
    assemblyConfiguration: string;
    rootNamespace: string;
};

export type UserRegistrationStatusModel = {
    isUserRegistered: boolean;
};

export type UserModel = {
    id: number;
    displayName: string;
    emailAddress: string;
    externalUserId: string;
};

export type SurveyModel = {
    id: number;
    topic: string;
    respondentType: string;
    numberOfRespondents: number;
    createdOn: Date;
    options: SurveyOptionModel[];
};

export type UserSurveyModel = {
    id: number;
    topic: string;
    respondentType: string;
    numberOfRespondents: number;
    numberOfOptions: number;
    winningOption: string;
    winningOptionNumberOfVotes: number;
};

export type SurveyOptionModel = {
    id: number;
    optionText: string;
    numberOfVotes: number;
    preferredNumberOfVotes: number;
};

export type CreateSurveyCommand = {
    surveyTopic: string;
    numberOfRespondents: number;
    respondentType: string;
    surveyOptions: SurveyOptionDto[];
};

export type SurveyOptionDto = {
    optionText: string;
    preferredNumberOfVotes: number;
};
