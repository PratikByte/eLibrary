/**
 * TEMPORARY visual placeholder data.
 * Used only to design the pages. It will be replaced by real backend
 * API responses later — nothing here is real library data.
 */

export type Book = {
  id: number;
  title: string;
  author: string;
  /** Present on dummy data and the price-range API; omitted by other book endpoints. */
  price?: number;
  /** Dummy data only — the backend does not expose total copies. */
  total?: number;
  available: number;
  /** Optional cover image URL from the API or a local preview. */
  cover?: string;
};

export const sampleBooks: Book[] = [
  { id: 1, title: "The Silent Library", author: "Ana Marsh", price: 320, total: 6, available: 4 },
  { id: 2, title: "Clean Foundations", author: "R. Kapoor", price: 540, total: 4, available: 0 },
  { id: 3, title: "Atlas of Small Things", author: "Julio Vera", price: 275, total: 8, available: 5 },
  { id: 4, title: "Green Architecture", author: "Mei Tanaka", price: 610, total: 3, available: 1 },
  { id: 5, title: "Letters from Marrow", author: "P. Osei", price: 190, total: 10, available: 9 },
  { id: 6, title: "The Paper Engine", author: "L. Fontaine", price: 430, total: 5, available: 2 },
];

export type Member = {
  id: string;
  name: string;
  email: string;
  role: "Admin" | "Member";
  loans: number;
  joined: string;
  /** Optional profile picture URL (local preview until the API is connected). */
  avatar?: string;
};

export const sampleMembers: Member[] = [
  { id: "1", name: "Aarav Shah", email: "aarav@example.com", role: "Member", loans: 2, joined: "12 Jan 2026" },
  { id: "2", name: "Priya Nair", email: "priya@example.com", role: "Member", loans: 0, joined: "03 Feb 2026" },
  { id: "3", name: "Daniel Cruz", email: "daniel@example.com", role: "Admin", loans: 1, joined: "22 Nov 2025" },
  { id: "4", name: "Sara Khan", email: "sara@example.com", role: "Member", loans: 3, joined: "08 Mar 2026" },
];

export type Loan = {
  id: number;
  title: string;
  member: string;
  borrowed: string;
  due: string;
  status: "Active" | "Overdue" | "Returned";
  fine: number;
};

export const sampleLoans: Loan[] = [
  { id: 1, title: "The Silent Library", member: "Aarav Shah", borrowed: "10 Aug 2026", due: "17 Aug 2026", status: "Overdue", fine: 20 },
  { id: 2, title: "Green Architecture", member: "Sara Khan", borrowed: "14 Aug 2026", due: "21 Aug 2026", status: "Active", fine: 0 },
  { id: 3, title: "Letters from Marrow", member: "Priya Nair", borrowed: "01 Aug 2026", due: "08 Aug 2026", status: "Returned", fine: 0 },
  { id: 4, title: "The Paper Engine", member: "Aarav Shah", borrowed: "15 Aug 2026", due: "22 Aug 2026", status: "Active", fine: 0 },
];

export const recentActivity = [
  { id: 1, text: "Sara Khan borrowed Green Architecture", time: "2 hours ago" },
  { id: 2, text: "Priya Nair returned Letters from Marrow", time: "Yesterday" },
  { id: 3, text: "New title added: Atlas of Small Things", time: "2 days ago" },
  { id: 4, text: "Aarav Shah renewed The Paper Engine", time: "3 days ago" },
];
