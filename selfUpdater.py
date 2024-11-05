import git
import os

repo_path = os.path.dirname(os.path.abspath(__file__))
repo = git.Repo(repo_path)

origin = repo.remotes.origin
origin.fetch()
