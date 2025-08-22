import axios from "axios"; import { store } from "../store"; import { refreshThunk, logout } from "./authSlice";
const api = axios.create({ baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5113/api" });

api.interceptors.request.use((config)=>{ const s:any = store.getState(); const t=s.auth.accessToken; if(t) config.headers.Authorization=`Bearer ${t}`; return config; });

let refreshing=false; let queue:Array<()=>void>=[];
api.interceptors.response.use(r=>r, async err=>{
  const original = err.config;
  if(err?.response?.status===401 && !original._retry){
    original._retry=true;
    if(!refreshing){
      refreshing=true;
      try{ await store.dispatch<any>(refreshThunk()); queue.forEach(f=>f()); queue=[]; return api(original); }
      catch{ store.dispatch(logout()); throw err; }
      finally{ refreshing=false; }
    }
    return new Promise(res=>queue.push(()=>res(api(original))));
  }
  throw err;
});
export default api;
