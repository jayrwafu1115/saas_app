"use client";

import { ReactNode, useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { ShieldCheck } from "lucide-react";
import { useAuthStore } from "@/store/auth-store";

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const accessToken = useAuthStore((state) => state.accessToken);
  const hasHydrated = useAuthStore((state) => state.hasHydrated);

  useEffect(() => {
    if (hasHydrated && !accessToken) {
      router.replace(`/login?returnTo=${encodeURIComponent(pathname)}`);
    }
  }, [accessToken, hasHydrated, pathname, router]);

  if (!hasHydrated || !accessToken) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-background text-foreground">
        <div className="flex items-center gap-3 text-sm text-muted-foreground">
          <ShieldCheck className="h-5 w-5 text-accent" aria-hidden="true" />
          Checking access
        </div>
      </main>
    );
  }

  return children;
}
