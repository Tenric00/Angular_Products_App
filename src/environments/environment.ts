// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001' // update if your backend uses a different port
};

export const environmentVariables = {
    apiUrl: environment.apiUrl
};
