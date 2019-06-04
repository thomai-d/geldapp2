// This file can be replaced during build by using the `fileReplacements` array.
// `ng build ---prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,

  // App version. This value is replaced by CI server on build.
  version: '0.0.0',

  // Release date and time.
  versionDate: '-',

  // Milliseconds after which the local category cache is
  // invalidated and fetched again from the server.
  categoryCacheInvalidationIntervalMs: 60000,

  // Number of items per page in the expense-list
  expenseItemsPerPage: 5
};

/*
 * In development mode, to ignore zone related error stack frames such as
 * `zone.run`, `zoneDelegate.invokeTask` for easier debugging, you can
 * import the following file, but please comment it out in production mode
 * because it will have performance impact when throw error
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
