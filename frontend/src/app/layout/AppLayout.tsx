import { NavLink, Outlet } from "react-router-dom";
import { Upload, ListTodo, Camera } from "lucide-react";

export function AppLayout() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-900 to-slate-800">
      {/* Header */}
      <header className="sticky top-0 z-50 backdrop-blur-lg bg-slate-900/80 border-b border-slate-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <div className="flex items-center gap-3">
              <div className="p-2 bg-gradient-to-br from-primary-500 to-violet-600 rounded-xl">
                <Camera className="w-6 h-6 text-white" />
              </div>
              <span className="text-xl font-bold bg-gradient-to-r from-primary-400 to-violet-400 bg-clip-text text-transparent">
                RapidPhotoFlow
              </span>
            </div>

            {/* Navigation */}
            <nav className="flex items-center gap-1">
              <NavLink
                to="/upload"
                className={({ isActive }) =>
                  `flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                    isActive
                      ? "bg-primary-600/20 text-primary-400"
                      : "text-slate-400 hover:text-slate-100 hover:bg-slate-800"
                  }`
                }
              >
                <Upload className="w-4 h-4" />
                Upload
              </NavLink>
              <NavLink
                to="/queue"
                className={({ isActive }) =>
                  `flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                    isActive
                      ? "bg-primary-600/20 text-primary-400"
                      : "text-slate-400 hover:text-slate-100 hover:bg-slate-800"
                  }`
                }
              >
                <ListTodo className="w-4 h-4" />
                Queue
              </NavLink>
            </nav>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Outlet />
      </main>

      {/* Footer */}
      <footer className="border-t border-slate-800 mt-auto">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <p className="text-center text-sm text-slate-500">
            TeamFront AI Hackathon — RapidPhotoFlow
          </p>
        </div>
      </footer>
    </div>
  );
}

