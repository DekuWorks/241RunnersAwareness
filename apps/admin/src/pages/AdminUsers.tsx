import { useEffect, useState } from "react"; import api from "../store/api";
type U = { id:string; email:string; displayName:string; isDisabled:boolean; };
export default function AdminUsers(){
  const [q,setQ]=useState(""); const [list,setList]=useState<U[]>([]); const [loading,setLoading]=useState(false);
  async function load(){ setLoading(true); const {data}=await api.get(`/admin/users?q=${encodeURIComponent(q)}`); setList(data.items); setLoading(false); }
  useEffect(()=>{ load(); },[]);
  return <div className="container">
    <h1 style={{margin:0}}>Users</h1>
    <div style={{display:"flex",gap:8,margin:"12px 0"}}>
      <input placeholder="Search email/name" value={q} onChange={e=>setQ(e.target.value)} style={{padding:10,borderRadius:10,border:"1px solid #1f2937",background:"transparent",color:"var(--text)"}}/>
      <button className="btn" onClick={load} disabled={loading}>{loading?"Loading…":"Search"}</button>
    </div>
    <div className="card">
      <table style={{width:"100%",borderCollapse:"collapse"}}>
        <thead><tr><th align="left">Email</th><th align="left">Name</th><th>Status</th><th></th></tr></thead>
        <tbody>
          {list.map(u=> <tr key={u.id}>
            <td>{u.email}</td><td>{u.displayName}</td>
            <td>{u.isDisabled? "Disabled":"Active"}</td>
            <td><a href="#" onClick={async e=>{e.preventDefault(); await api.post(`/admin/users/${u.id}/disable`); load();}}>Disable</a></td>
          </tr>)}
        </tbody>
      </table>
    </div>
  </div>
}
