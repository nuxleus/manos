
programfilesdir=@libdir@/@PACKAGE@/

programfiles_DATA = $(top_srcdir)/build/Manos.Tests.dll

manos_proj_file = Manos.Tests.csproj
manos_sources_in = manos.tests.pc.in
manos_sources = $(manos_proj_file) $(shell  xbuild /verbosity:minimal /target:ListSources | sed -e '1,3d' -e "s/\;/ /g")

EXTRA_DIST = $(manos_sources) $(manos_sources_in)
CLEANFILES = $(top_srcdir)/build/Manos.Tests.dll

all: $(top_srcdir)/build/Manos.Tests.dll

$(top_srcdir)/build/Manos.Tests.dll: $(manos_sources)
	$(XBUILD) $(srcdir)/$(manos_proj_file)
