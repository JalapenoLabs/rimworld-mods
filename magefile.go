//go:build mage

// Magefile for the JalapenoLabs RimWorld mod monorepo.
//
// Targets:
//
//	mage build <mod>   – compile a single mod (e.g. mage build electricity-meter)
//	mage buildAll      – compile every mod under mods/
//	mage clean <mod>   – remove compiled assemblies for a single mod
//	mage cleanAll      – remove compiled assemblies for every mod
package main

import (
	"fmt"
	"os"
	"path/filepath"

	"github.com/magefile/mage/sh"
)

// Build compiles a single mod by name.
// Cleans its existing assemblies first so stale DLLs never linger.
//
//	mage build electricity-meter
func Build(mod string) error {
	csproj := filepath.Join("mods", mod, "mod.csproj")
	if _, err := os.Stat(csproj); os.IsNotExist(err) {
		return fmt.Errorf("mod %q not found — expected %s to exist", mod, csproj)
	}

	if err := Clean(mod); err != nil {
		return err
	}

	fmt.Printf("Building %s...\n", mod)
	return sh.Run("dotnet", "build", csproj)
}

// BuildAll compiles every mod found under mods/.
func BuildAll() error {
	mods, err := discoverMods()
	if err != nil {
		return err
	}
	for _, mod := range mods {
		if err := Build(mod); err != nil {
			return fmt.Errorf("%s: %w", mod, err)
		}
	}
	return nil
}

// Clean removes compiled assemblies for a single mod.
//
//	mage clean electricity-meter
func Clean(mod string) error {
	dir := filepath.Join("mods", mod, "1.6", "Assemblies")
	return removeFiles(dir)
}

// CleanAll removes compiled assemblies for every mod under mods/.
func CleanAll() error {
	mods, err := discoverMods()
	if err != nil {
		return err
	}
	for _, mod := range mods {
		if err := Clean(mod); err != nil {
			return fmt.Errorf("%s: %w", mod, err)
		}
	}
	return nil
}

// discoverMods returns the name of every subdirectory under mods/ that
// contains a mod.csproj — the canonical indicator of a buildable mod.
func discoverMods() ([]string, error) {
	entries, err := os.ReadDir("mods")
	if err != nil {
		return nil, fmt.Errorf("reading mods/: %w", err)
	}
	var mods []string
	for _, e := range entries {
		if !e.IsDir() {
			continue
		}
		csproj := filepath.Join("mods", e.Name(), "mod.csproj")
		if _, err := os.Stat(csproj); err == nil {
			mods = append(mods, e.Name())
		}
	}
	return mods, nil
}

// removeFiles deletes all files (not directories) inside dir.
// Returns nil if the directory does not exist — nothing to clean.
func removeFiles(dir string) error {
	entries, err := os.ReadDir(dir)
	if os.IsNotExist(err) {
		return nil
	}
	if err != nil {
		return fmt.Errorf("reading %s: %w", dir, err)
	}
	for _, e := range entries {
		if e.IsDir() {
			continue
		}
		path := filepath.Join(dir, e.Name())
		if err := os.Remove(path); err != nil {
			return fmt.Errorf("removing %s: %w", path, err)
		}
	}
	return nil
}
