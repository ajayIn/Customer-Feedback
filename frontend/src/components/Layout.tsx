import { Link, useNavigate } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { clearSession, selectAuth } from '../app/authSlice'

export function Layout({ children }: { children: React.ReactNode }) {
  const auth = useSelector(selectAuth)
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const isManager = auth.user?.roles.includes('StoreManager')
  const isCustomer = auth.user?.roles.includes('Customer')

  return (
    <div className="app">
      <header className="header">
        <Link to={isManager ? '/' : isCustomer ? '/feedback/new' : '/login'} className="brand">
          Customer Feedback
        </Link>
        <nav>
          {!auth.user && (
            <>
              <Link to="/login">Login</Link>
              <Link to="/register">Register</Link>
            </>
          )}
          {isCustomer && (
            <>
              <Link to="/feedback/new">Submit feedback</Link>
              <Link to="/feedback/mine">My feedback</Link>
            </>
          )}
          {isManager && (
            <>
              <Link to="/">Dashboard</Link>
              <Link to="/manager/feedback">Manage</Link>
            </>
          )}
          {auth.user && (
            <button
              type="button"
              className="link-btn"
              onClick={() => {
                dispatch(clearSession())
                navigate('/login')
              }}
            >
              Log out · {auth.user.displayName}
            </button>
          )}
        </nav>
      </header>
      <main className="main">{children}</main>
    </div>
  )
}
