import { HttpContextToken } from '@angular/common/http';

export const SKIP_ERROR_INTERCEPTOR = new HttpContextToken(() => false);
