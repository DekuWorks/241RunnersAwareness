import { createSlice, createAsyncThunk } from "@reduxjs/toolkit"; import api from "./api";
type S = { accessToken?:string; refreshToken?:string; user?: any; };
const initialState:S = {};

export const loginThunk = createAsyncThunk("auth/login", async (p:{email:string;password:string})=>{
  const {data} = await api.post("/auth/login",{...p, device:"web"}); return data;
});
export const meThunk = createAsyncThunk("auth/me", async ()=>{ const {data} = await api.get("/auth/me"); return data.user; });
export const refreshThunk = createAsyncThunk("auth/refresh", async (_,{getState})=>{
  const {auth}:any = getState(); const {data} = await api.post("/auth/refresh",{ refreshToken: auth.refreshToken }); return data;
});

const slice = createSlice({
  name:"auth", initialState,
  reducers:{ logout(s){ s.accessToken=undefined; s.refreshToken=undefined; s.user=undefined; localStorage.removeItem("auth"); } },
  extraReducers:(b)=>{
    b.addCase(loginThunk.fulfilled,(s,a)=>{ Object.assign(s,a.payload); localStorage.setItem("auth",JSON.stringify(s)); });
    b.addCase(meThunk.fulfilled,(s,a)=>{ s.user=a.payload; localStorage.setItem("auth",JSON.stringify(s)); });
    b.addCase(refreshThunk.fulfilled,(s,a)=>{ s.accessToken=a.payload.accessToken; s.refreshToken=a.payload.refreshToken; localStorage.setItem("auth",JSON.stringify(s)); });
  }
});
export const { logout } = slice.actions; export default slice.reducer;
