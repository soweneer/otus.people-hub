export interface MeResponse {
  authenticated: boolean;
  email: string | null;
}

export interface UserLite {
  id: number;
  name: string;
  age: number;
  city: string;
}

// Соответствует PeopleHub.Domain.Enums.FriendRequestStatus
export const FriendRequestStatus = {
  None: -1,
  Sent: 0,
  Rejected: 1,
  Approved: 2,
} as const;

export type FriendRequestStatus = (typeof FriendRequestStatus)[keyof typeof FriendRequestStatus];

// Соответствует PeopleHub.Domain.Enums.Gender
export const Gender = {
  Female: 0,
  Male: 1,
} as const;

export type Gender = (typeof Gender)[keyof typeof Gender];

export interface UserInfo {
  user: UserLite;
  status: FriendRequestStatus;
}

export interface FriendInfoLite {
  user: UserLite;
  friendRequestId: number;
}

export interface FriendsInfo {
  friends: FriendInfoLite[];
  incoming: FriendInfoLite[];
  outgoing: FriendInfoLite[];
}

export interface FriendInfo {
  id: number;
  name: string;
  surname: string;
  age: number;
  city: string;
  gender: Gender;
  bio: string | null;
  status: FriendRequestStatus;
}

export interface PersonalInfo {
  name: string;
  surname: string;
  age: number;
  city: string;
  bio: string | null;
  gender: Gender;
}

export interface FeedPost {
  id: number;
  text: string;
  authorUserId: number;
}

export interface PostFeedItemResponse {
  id: string;
  text: string;
  author_user_id: string;
}

export interface DialogPartner {
  id: number;
  name: string;
}

export interface DialogMessage {
  from: string;
  to: string;
  text: string;
}

export interface SignUpData {
  name: string;
  surname: string;
  city: string;
  email: string;
  password: string;
  confirmPassword: string;
  bio: string;
  age: number;
  gender: number;
}

export interface ProfileData {
  name: string;
  surname: string;
  age: number;
  city: string;
  bio: string;
  gender: number;
}
