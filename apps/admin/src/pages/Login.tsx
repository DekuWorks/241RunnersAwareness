import { useState } from "react"; import { useDispatch } from "react-redux"; import { AppDispatch } from "../types";
import { loginThunk, meThunk } from "../store/authSlice"; import { useNavigate } from "react-router-dom";
export default function Login(){
  const [email,setEmail]=useState(""); const [password,setPassword]=useState(""); const [err,setErr]=useState("");
  const nav=useNavigate(); const d = useDispatch<AppDispatch>();
  async function onSubmit(e:any){ e.preventDefault(); setErr("");
    try{ await d(loginThunk({email,password})); await d(meThunk()); nav("/dashboard"); } catch{ setErr("Invalid credentials"); }
  }
  return <div className="container" style={{minHeight:"70vh",display:"grid",placeItems:"center"}}>
    <form onSubmit={onSubmit} className="card" style={{minWidth:360}}>
      <h2 style={{marginTop:0}}>Sign in</h2>
      <div style={{display:"grid",gap:8}}>
        <input placeholder="Email" value={email} onChange={e=>setEmail(e.target.value)} style={{padding:10,borderRadius:10,border:"1px solid #1f2937",background:"transparent",color:"var(--text)"}}/>
        <input type="password" placeholder="Password" value={password} onChange={e=>setPassword(e.target.value)} style={{padding:10,borderRadius:10,border:"1px solid #1f2937",background:"transparent",color:"var(--text)"}}/>
        {err && <div className="badge" style={{color:"salmon"}}>{err}</div>}
        <button className="btn" type="submit">Continue</button>
      </div>
      
      {/* Demo Credentials */}
      <div style={{marginTop:24,padding:16,background:"rgba(255,255,255,.03)",borderRadius:10,border:"1px solid rgba(255,255,255,.08)"}}>
        <h4 style={{margin:"0 0 12px 0",fontSize:14,color:"var(--muted)"}}>Demo Credentials</h4>
        <div style={{display:"grid",gap:8,fontSize:13}}>
          <div>
            <strong>Admin:</strong> admin@example.com / ChangeMe123!
          </div>
          <div style={{color:"var(--muted)",fontSize:12}}>
            Full access to all features including user management
          </div>
        </div>
        <button 
          type="button" 
          onClick={() => { setEmail("admin@example.com"); setPassword("ChangeMe123!"); }}
          style={{
            marginTop:12,
            padding:"6px 12px",
            fontSize:12,
            background:"transparent",
            border:"1px solid var(--accent)",
            color:"var(--accent)",
            borderRadius:6,
            cursor:"pointer"
          }}
        >
          Use Demo Admin
        </button>
      </div>
    </form>
  </div>
}
