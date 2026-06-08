import { NavLink } from 'react-router-dom'
import { cn } from '@/lib/utils'
import {
  BarChart2,
  ChartNoAxesColumn,
  Database,
  Heart,
  LayoutDashboard,
  LineChart,
  ListChecks,
} from 'lucide-react'

interface NavItem {
  to: string
  icon: React.ReactNode
  label: string
}

const navItems: NavItem[] = [
  { to: '/', icon: <LayoutDashboard size={16} />, label: 'დაფა' },
  { to: '/health', icon: <Heart size={16} />, label: 'ჯანმრთელობის ინდექსი' },
  { to: '/market', icon: <ListChecks size={16} />, label: 'ბაზარი და კონკურენცია' },
  { to: '/program', icon: <BarChart2 size={16} />, label: 'პროგრამის ანალიზი' },
  { to: '/trends', icon: <LineChart size={16} />, label: 'ტენდენციები' },
  { to: '/compare', icon: <ChartNoAxesColumn size={16} />, label: 'პროგრამების შედარება' },
  { to: '/data', icon: <Database size={16} />, label: 'მონაცემთა მართვა' },
]

export function Sidebar({ onNavigate }: { onNavigate?: () => void }) {
  return (
    <aside className="flex h-screen w-60 flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground shrink-0">

      <div className="flex items-center gap-2.5 border-b border-sidebar-border px-4 py-3.5">
        <div className="flex h-7 w-7 items-center justify-center rounded bg-sidebar-primary/20 text-sidebar-primary shrink-0">
          <LayoutDashboard size={14} />
        </div>
        <div className="min-w-0">
          <p className="text-xs font-semibold text-sidebar-foreground leading-tight">
            მისაღები გამოცდები
          </p>
          <p className="text-[10px] text-sidebar-foreground/50 leading-tight">ანალიტიკა</p>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto py-2 px-1.5 space-y-0.5">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === '/'}
            onClick={onNavigate}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-2.5 rounded px-2.5 py-1.5 text-[12.5px] transition-colors',
                isActive
                  ? 'bg-sidebar-accent font-semibold text-sidebar-accent-foreground border-l-2 border-sidebar-primary pl-[9px]'
                  : 'text-sidebar-foreground/65 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground',
              )
            }
          >
            <span className="shrink-0">{item.icon}</span>
            <span className="truncate">{item.label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="border-t border-sidebar-border px-4 py-2.5">
        <p className="text-[10px] text-sidebar-foreground/40">NAEC · 2025</p>
      </div>
    </aside>
  )
}
