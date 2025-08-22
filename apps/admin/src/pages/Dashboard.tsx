import { useSelector } from "react-redux";
export default function Dashboard(){
  const user = useSelector((s:any)=>s.auth.user);
  return <div className="container">
    <div style={{display:"flex",justifyContent:"space-between",alignItems:"center"}}>
      <h1 style={{margin:0}}>Dashboard</h1>
      <div className="badge">{user?.email} · {user?.roles?.join(", ")}</div>
    </div>
    <div className="grid" style={{display:"grid",gap:16,gridTemplateColumns:"repeat(auto-fit,minmax(260px,1fr))",marginTop:16}}>
      <div className="card"><h3>Status</h3><p>All systems normal.</p></div>
      <div className="card"><h3>Users</h3><a href="/admin/users">Manage users →</a></div>
    </div>
  </div>
}
