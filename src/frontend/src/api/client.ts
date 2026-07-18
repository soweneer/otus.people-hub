import type {
  FeedPost,
  FriendInfo,
  FriendsInfo,
  MeResponse,
  PersonalInfo,
  PostFeedItemResponse,
  ProfileData,
  SignUpData,
  UserInfo,
} from './types';

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

async function extractError(res: Response): Promise<string> {
  try {
    const data = await res.json();
    if (typeof data === 'string') return data;
    if (data?.error) return data.error;
    // ValidationProblemDetails от [ApiController]
    if (data?.errors) return Object.values<string[]>(data.errors).flat().join('; ');
    if (data?.title) return data.title;
  } catch {
    // тело не JSON — вернём общий текст
  }
  return `Ошибка запроса (${res.status})`;
}

async function request<T>(
  url: string,
  options: RequestInit = {},
  { notifyUnauthorized = true }: { notifyUnauthorized?: boolean } = {},
): Promise<T> {
  const res = await fetch(url, {
    credentials: 'same-origin',
    ...options,
    headers: {
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
  });

  if (!res.ok) {
    if (res.status === 401 && notifyUnauthorized) {
      window.dispatchEvent(new Event('auth:unauthorized'));
    }
    throw new ApiError(await extractError(res), res.status);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

export const authApi = {
  me: () => request<MeResponse>('/api/auth/me'),
  signIn: (email: string, password: string) =>
    request<MeResponse>('/api/auth/signin', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),
  signUp: (data: SignUpData) =>
    request<MeResponse>('/api/auth/signup', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  signOut: () => request<void>('/api/auth/signout', { method: 'POST' }),
};

export const usersApi = {
  search: (firstName: string, lastName: string, skip: number, take: number) => {
    const params = new URLSearchParams();
    if (firstName) params.set('firstName', firstName);
    if (lastName) params.set('lastName', lastName);
    params.set('skip', String(skip));
    params.set('take', String(take));
    return request<UserInfo[]>(`/api/users?${params}`);
  },
  getById: (id: number) => request<FriendInfo>(`/api/users/${id}`),
};

export const profileApi = {
  get: () => request<PersonalInfo>('/api/profile'),
  update: (data: ProfileData) =>
    request<PersonalInfo>('/api/profile', {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
};

export const friendsApi = {
  get: () => request<FriendsInfo>('/api/friends'),
  sendRequest: (targetUserId: number) =>
    request<void>('/api/friends/requests', {
      method: 'POST',
      body: JSON.stringify({ targetUserId }),
    }),
  cancel: (userId: number) => request<void>(`/api/friends/${userId}`, { method: 'DELETE' }),
  approve: (requestId: number) =>
    request<void>(`/api/friends/requests/${requestId}/approve`, { method: 'POST' }),
  reject: (requestId: number) =>
    request<void>(`/api/friends/requests/${requestId}/reject`, { method: 'POST' }),
};

export const feedApi = {
    return posts.map((p) => ({
      id: Number(p.id),
      text: p.text,
      authorUserId: Number(p.author_user_id),
    }));
  },
};
